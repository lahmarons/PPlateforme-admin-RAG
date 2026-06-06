# ============================================================
# chatbot/core/embeddings.py
# Service Sentence Transformers — conforme Chapitre 4.4.2
# Modèle multilingue : support arabe + français
# ============================================================
import time
import numpy as np
from typing import List, Optional
from pathlib import Path

import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))
from config.settings import EMBEDDING_MODEL


class EmbeddingService:
    """
    Génère des embeddings vectoriels via Sentence Transformers.
    
    Modèle utilisé : paraphrase-multilingual-MiniLM-L12-v2
    - Supporte 50+ langues (arabe, français, anglais...)
    - Dimension : 384
    - Rapide et léger (120MB)
    
    Référence rapport : Section 4.4.2 — "Sentence Transformers :
    bibliothèque utilisée pour transformer les textes en vecteurs
    numériques (embeddings)"
    """

    _instance: Optional["EmbeddingService"] = None
    _model = None

    def __new__(cls):
        """Singleton — un seul modèle chargé en mémoire"""
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

    def load(self):
        """Charge le modèle Sentence Transformers (lazy loading)"""
        if self._model is not None:
            return

        print(f"[EmbeddingService] Chargement du modèle: {EMBEDDING_MODEL}")
        try:
            from sentence_transformers import SentenceTransformer
            self._model = SentenceTransformer(EMBEDDING_MODEL)
            print(f"[EmbeddingService] ✅ Modèle chargé — dimension: {self.get_dimension()}")
        except ImportError:
            print("[EmbeddingService] ⚠️  sentence-transformers non installé")
            self._model = None
        except Exception as e:
            print(f"[EmbeddingService] ❌ Erreur: {e}")
            self._model = None

    def is_ready(self) -> bool:
        return self._model is not None

    def get_dimension(self) -> int:
        """Retourne la dimension des vecteurs"""
        if self._model:
            return self._model.get_sentence_embedding_dimension()
        return 384  # default MiniLM

    def encode(self, texts: List[str]) -> np.ndarray:
        """
        Encode une liste de textes en vecteurs numpy.
        
        Args:
            texts: Liste de textes à encoder
            
        Returns:
            Array numpy (n_texts, dimension)
        """
        if not self.is_ready():
            self.load()

        if not self.is_ready():
            # Mode dégradé : vecteurs aléatoires pour les tests
            print("[EmbeddingService] Mode dégradé — vecteurs aléatoires")
            return np.random.rand(len(texts), 384).astype("float32")

        embeddings = self._model.encode(
            texts,
            convert_to_numpy=True,
            normalize_embeddings=True,   # Normalisation L2 pour cosine similarity
            show_progress_bar=False,
            batch_size=32
        )
        return embeddings.astype("float32")

    def encode_single(self, text: str) -> np.ndarray:
        """Encode un seul texte"""
        return self.encode([text])[0]

    def similarity(self, vec1: np.ndarray, vec2: np.ndarray) -> float:
        """Calcule la similarité cosinus entre deux vecteurs"""
        return float(np.dot(vec1, vec2) / (np.linalg.norm(vec1) * np.linalg.norm(vec2)))


# Singleton global
embedding_service = EmbeddingService()
