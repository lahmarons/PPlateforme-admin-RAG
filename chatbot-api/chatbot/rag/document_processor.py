# ============================================================
# chatbot/rag/document_processor.py
# Pipeline d'indexation documentaire — conforme Chapitre 4.3.2
# Data Indexing: Document → Text Chunks → Embeddings → Vector DB
# ============================================================
import re
import time
from typing import List, Tuple
from pathlib import Path

import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))
from config.settings import CHUNK_SIZE, CHUNK_OVERLAP, DOCUMENTS_DIR
from chatbot.core.vector_store import vector_store, Chunk


class DocumentProcessor:
    """
    Traitement des documents administratifs pour le RAG.
    
    Pipeline (Chapitre 4.3.2 — Basic RAG Pipeline) :
    Documents → Data Loading → Data Splitting → Data Embedding → Data Storing
    
    Formats supportés : PDF, TXT, DOCX
    """

    SUPPORTED_FORMATS = [".pdf", ".txt", ".docx", ".md"]

    # ── Chargement ────────────────────────────────────────────

    def load_pdf(self, file_path: Path) -> List[Tuple[str, int]]:
        """
        Charge un PDF et retourne [(texte, page), ...]
        
        Returns:
            Liste de (texte_page, numéro_page)
        """
        try:
            from pypdf import PdfReader
            reader = PdfReader(str(file_path))
            pages = []
            for i, page in enumerate(reader.pages):
                text = page.extract_text() or ""
                text = self._clean_text(text)
                if text.strip():
                    pages.append((text, i + 1))
            print(f"[Processor] PDF chargé: {len(pages)} pages — {file_path.name}")
            return pages
        except Exception as e:
            print(f"[Processor] ❌ Erreur PDF: {e}")
            return []

    def load_txt(self, file_path: Path) -> List[Tuple[str, int]]:
        """Charge un fichier texte"""
        try:
            text = file_path.read_text(encoding="utf-8", errors="ignore")
            text = self._clean_text(text)
            return [(text, 1)]
        except Exception as e:
            print(f"[Processor] ❌ Erreur TXT: {e}")
            return []

    def load_docx(self, file_path: Path) -> List[Tuple[str, int]]:
        """Charge un fichier Word"""
        try:
            from docx import Document
            doc = Document(str(file_path))
            text = "\n".join(p.text for p in doc.paragraphs if p.text.strip())
            text = self._clean_text(text)
            return [(text, 1)]
        except Exception as e:
            print(f"[Processor] ❌ Erreur DOCX: {e}")
            return []

    def load_file(self, file_path: Path) -> List[Tuple[str, int]]:
        """Charge un fichier selon son extension"""
        ext = file_path.suffix.lower()
        loaders = {
            ".pdf":  self.load_pdf,
            ".txt":  self.load_txt,
            ".md":   self.load_txt,
            ".docx": self.load_docx,
        }
        loader = loaders.get(ext)
        if loader is None:
            raise ValueError(f"Format non supporté: {ext}")
        return loader(file_path)

    # ── Chunking ──────────────────────────────────────────────

    def split_text(
        self,
        text: str,
        chunk_size: int = CHUNK_SIZE,
        overlap: int = CHUNK_OVERLAP
    ) -> List[Tuple[str, int, int]]:
        """
        Découpe le texte en chunks avec overlap.
        
        Returns:
            Liste de (texte_chunk, char_start, char_end)
        """
        # Séparateurs prioritaires (du plus structurant au moins)
        separators = ["\n\n", "\n", ". ", "، ", " "]

        chunks = []
        start = 0
        text_len = len(text)

        while start < text_len:
            end = min(start + chunk_size, text_len)

            # Chercher le meilleur point de coupure
            if end < text_len:
                best_sep_pos = -1
                for sep in separators:
                    pos = text.rfind(sep, start, end)
                    # Ne choisir le séparateur que s'il permet d'avancer de manière significative
                    # (évite les chevauchements de 1 caractère s'il y a un séparateur très proche du début)
                    if pos > start + overlap:
                        best_sep_pos = pos + len(sep)
                        break
                if best_sep_pos > start:
                    end = best_sep_pos

            chunk_text = text[start:end].strip()
            if chunk_text:
                chunks.append((chunk_text, start, end))

            if end >= text_len:
                break
            start = max(start + 1, end - overlap)

        return chunks

    # ── Pipeline principal ────────────────────────────────────

    def process_file(
        self,
        file_path: Path,
        source_filename: str
    ) -> int:
        """
        Pipeline complet d'indexation d'un document.
        
        Étapes (Chapitre 4.3.2) :
        1. Data Loading   → Charger le PDF/DOCX
        2. Data Splitting → Découper en chunks
        3. Data Embedding → Générer les embeddings (via vector_store)
        4. Data Storing   → Sauvegarder dans FAISS
        
        Returns:
            Nombre de chunks créés
        """
        t0 = time.time()
        print(f"\n[Processor] === Traitement: {source_filename} ===")

        # 1. Data Loading
        pages = self.load_file(file_path)
        if not pages:
            return 0

        # 2. Data Splitting
        all_chunks: List[Chunk] = []
        chunk_idx = 0

        for page_text, page_num in pages:
            raw_chunks = self.split_text(page_text)

            for chunk_text, char_start, char_end in raw_chunks:
                if len(chunk_text.strip()) < 20:  # Ignorer les chunks trop courts
                    continue

                chunk = Chunk(
                    id=0,  # Assigné par FAISS
                    text=chunk_text,
                    source_file=source_filename,
                    page=page_num,
                    chunk_index=chunk_idx,
                    char_start=char_start,
                    char_end=char_end
                )
                all_chunks.append(chunk)
                chunk_idx += 1

        print(f"[Processor] {len(all_chunks)} chunks créés depuis {len(pages)} pages")

        if not all_chunks:
            return 0

        # 3 & 4. Data Embedding + Data Storing (via FAISS)
        vector_store.initialize()
        added = vector_store.add_chunks(all_chunks)

        elapsed = time.time() - t0
        print(f"[Processor] ✅ {added} chunks indexés en {elapsed:.2f}s")
        return added

    # ── Utils ─────────────────────────────────────────────────

    def _clean_text(self, text: str) -> str:
        """Nettoie le texte extrait"""
        # Remplacer les espaces horizontaux multiples par un seul espace (conserve les sauts de ligne)
        text = re.sub(r'[ \t]+', ' ', text)
        # Limiter les sauts de ligne consécutifs à 2 maximum
        text = re.sub(r'\n{3,}', '\n\n', text)
        text = re.sub(r'[^\w\s\.\,\;\:\!\?\-\(\)\[\]\{\}\"\'\n\u0600-\u06FF]', '', text)
        return text.strip()

    def is_supported(self, filename: str) -> bool:
        return Path(filename).suffix.lower() in self.SUPPORTED_FORMATS


# Singleton
document_processor = DocumentProcessor()
