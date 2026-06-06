# ============================================================
# chatbot/core/vector_store.py
# Service FAISS — conforme Chapitre 4.4.2
# "FAISS : moteur de recherche vectorielle permettant d'effectuer
#  une recherche sémantique rapide dans la base documentaire"
# ============================================================
import os
import json
import time
import pickle
import numpy as np
from typing import List, Tuple, Optional, Dict
from pathlib import Path
from dataclasses import dataclass, asdict

import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))
from config.settings import VECTOR_STORE_DIR, TOP_K_RESULTS, SIMILARITY_THRESHOLD
from chatbot.core.embeddings import embedding_service


@dataclass
class Chunk:
    """Un chunk de texte avec ses métadonnées"""
    id: int
    text: str
    source_file: str
    page: int
    chunk_index: int
    char_start: int
    char_end: int


class FAISSVectorStore:
    """
    Base vectorielle FAISS pour la recherche sémantique.
    
    Architecture RAG (Chapitre 4.3.2) :
    1. Documents → Chunks → Embeddings → FAISS Index
    2. Query → Embedding → FAISS Search → Top-K Chunks
    3. Top-K Chunks + Query → LLM → Réponse
    
    Type d'index : IndexFlatIP (produit scalaire, vecteurs normalisés)
    = similarité cosinus après normalisation L2
    """

    INDEX_FILE    = "faiss.index"
    METADATA_FILE = "metadata.pkl"
    CONFIG_FILE   = "config.json"

    def __init__(self):
        self.index = None
        self.chunks: List[Chunk] = []
        self.dimension = embedding_service.get_dimension()
        self._loaded = False

    # ── Initialisation ────────────────────────────────────────

    def initialize(self):
        """Initialise ou charge l'index FAISS"""
        try:
            import faiss
            index_path = VECTOR_STORE_DIR / self.INDEX_FILE

            if index_path.exists():
                self._load_index()
            else:
                self._create_empty_index()

            self._loaded = True
            print(f"[FAISS] ✅ Index prêt — {len(self.chunks)} chunks, dim={self.dimension}")

        except ImportError:
            print("[FAISS] ⚠️  faiss-cpu non installé — mode dégradé")
            self._loaded = False
        except Exception as e:
            print(f"[FAISS] ❌ Erreur initialisation: {e}")
            self._loaded = False

    def _create_empty_index(self):
        """Crée un nouvel index FAISS vide"""
        import faiss
        self.index  = faiss.IndexFlatIP(self.dimension)  # Inner Product = cosine (vecteurs normalisés)
        self.chunks = []
        print(f"[FAISS] Nouvel index créé (dim={self.dimension})")

    def _load_index(self):
        """Charge l'index depuis le disque"""
        import faiss
        index_path    = VECTOR_STORE_DIR / self.INDEX_FILE
        metadata_path = VECTOR_STORE_DIR / self.METADATA_FILE

        self.index = faiss.read_index(str(index_path))

        if metadata_path.exists():
            with open(metadata_path, "rb") as f:
                self.chunks = pickle.load(f)

        print(f"[FAISS] Index chargé: {self.index.ntotal} vecteurs, {len(self.chunks)} chunks")

    def save(self):
        """Sauvegarde l'index et les métadonnées sur disque"""
        if self.index is None:
            return

        import faiss
        faiss.write_index(self.index, str(VECTOR_STORE_DIR / self.INDEX_FILE))

        with open(VECTOR_STORE_DIR / self.METADATA_FILE, "wb") as f:
            pickle.dump(self.chunks, f)

        config = {
            "dimension": self.dimension,
            "total_vectors": self.index.ntotal,
            "total_chunks": len(self.chunks),
            "embedding_model": str(embedding_service._model)
        }
        with open(VECTOR_STORE_DIR / self.CONFIG_FILE, "w") as f:
            json.dump(config, f, indent=2)

        print(f"[FAISS] ✅ Sauvegardé: {self.index.ntotal} vecteurs")

    # ── Indexation ────────────────────────────────────────────

    def add_chunks(self, chunks: List[Chunk]) -> int:
        """
        Ajoute des chunks dans l'index FAISS.
        
        Processus (Chapitre 4.3.2 — Data Indexing) :
        1. Textes → Embeddings (Sentence Transformers)
        2. Embeddings → FAISS Index
        3. Métadonnées → pickle
        
        Returns:
            Nombre de chunks ajoutés
        """
        if not self._loaded:
            self.initialize()

        if not chunks:
            return 0

        # 1. Générer les embeddings
        texts = [c.text for c in chunks]
        t0 = time.time()
        embeddings = embedding_service.encode(texts)
        embed_time = time.time() - t0
        print(f"[FAISS] Embeddings générés en {embed_time:.2f}s pour {len(texts)} chunks")

        # 2. Assigner les IDs séquentiels
        start_id = len(self.chunks)
        for i, chunk in enumerate(chunks):
            chunk.id = start_id + i

        # 3. Ajouter dans l'index FAISS
        self.index.add(embeddings)
        self.chunks.extend(chunks)

        # 4. Sauvegarder
        self.save()

        return len(chunks)

    def remove_by_source(self, source_file: str) -> int:
        """Supprime tous les chunks d'un fichier (reconstruction de l'index)"""
        if not self._loaded:
            return 0

        import faiss

        # Filtrer les chunks à garder
        kept_chunks = [c for c in self.chunks if c.source_file != source_file]
        removed = len(self.chunks) - len(kept_chunks)

        if removed == 0:
            return 0

        # Reconstruire l'index FAISS avec les chunks restants
        self._create_empty_index()

        if kept_chunks:
            texts = [c.text for c in kept_chunks]
            embeddings = embedding_service.encode(texts)
            self.index.add(embeddings)
            self.chunks = kept_chunks
            for i, chunk in enumerate(self.chunks):
                chunk.id = i

        self.save()
        print(f"[FAISS] {removed} chunks supprimés pour {source_file}")
        return removed

    # ── Recherche ─────────────────────────────────────────────

    def search(
        self,
        query: str,
        k: int = TOP_K_RESULTS,
        threshold: float = SIMILARITY_THRESHOLD
    ) -> List[Tuple[Chunk, float]]:
        """
        Recherche sémantique FAISS.
        
        Pipeline (Chapitre 4.3.2 — Data Retrieval & Generation) :
        1. Query → Vector Embedding
        2. Vector DB (FAISS) → Top-K Chunks
        3. Filter par score de similarité
        
        Args:
            query: Question de l'utilisateur
            k: Nombre de résultats
            threshold: Score minimum de similarité
            
        Returns:
            Liste de (Chunk, score) triée par pertinence
        """
        if not self._loaded or self.index is None or self.index.ntotal == 0:
            return []

        # 1. Encoder la requête
        query_embedding = embedding_service.encode_single(query).reshape(1, -1)

        # 2. Recherche FAISS
        k_actual = min(k, self.index.ntotal)
        scores, indices = self.index.search(query_embedding, k_actual)

        # 3. Filtrer et retourner
        results = []
        for score, idx in zip(scores[0], indices[0]):
            if idx == -1:
                continue
            if score < threshold:
                continue
            if idx < len(self.chunks):
                results.append((self.chunks[idx], float(score)))

        return sorted(results, key=lambda x: x[1], reverse=True)

    # ── Stats ─────────────────────────────────────────────────

    def get_stats(self) -> Dict:
        """Statistiques de l'index"""
        if not self._loaded or self.index is None:
            return {
                "status": "non_initialized",
                "total_vectors": 0,
                "total_chunks": 0,
                "dimension": self.dimension
            }

        index_path = VECTOR_STORE_DIR / self.INDEX_FILE
        size_mb = index_path.stat().st_size / (1024 * 1024) if index_path.exists() else 0

        sources = list(set(c.source_file for c in self.chunks))

        return {
            "status": "ready",
            "total_vectors": self.index.ntotal,
            "total_chunks": len(self.chunks),
            "dimension": self.dimension,
            "index_size_mb": round(size_mb, 2),
            "unique_sources": len(sources),
            "sources": sources
        }

    def is_ready(self) -> bool:
        return self._loaded and self.index is not None and self.index.ntotal > 0


# Singleton global
vector_store = FAISSVectorStore()
