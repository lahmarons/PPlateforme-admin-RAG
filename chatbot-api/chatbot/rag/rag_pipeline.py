# chatbot/rag/rag_pipeline.py
import time
from typing import List, Optional, Tuple
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from config.settings import TOP_K_RESULTS, SYSTEM_PROMPT, GREETING_WORDS
from chatbot.core.vector_store import vector_store, Chunk
from chatbot.models.schemas import Source, RAGContext

DOCUMENT_TITLES = {
    "01_documents_identite":    "Documents d'identité — Ministère de l'Intérieur",
    "02_securite_sociale":      "Sécurité sociale — CNSS & CNAM",
    "03_enseignement_superieur":"Enseignement supérieur",
    "04_etat_civil_municipal":  "État civil — Services municipaux",
    "05_emploi_entrepreneuriat":"Emploi & Entrepreneuriat — ANETI",
}

def get_title(filename: str) -> str:
    name = Path(filename).stem.split("(")[0].strip()
    for key, title in DOCUMENT_TITLES.items():
        if key in name:
            return title
    return name.replace("_", " ").title()

def is_greeting(text: str) -> bool:
    """Détecte si le message est une salutation — pas besoin de RAG"""
    t = text.lower().strip()
    # Court message de salutation
    if len(t.split()) <= 3:
        for word in GREETING_WORDS:
            if word in t:
                return True
    return False


class RAGPipeline:

    def retrieve(self, query: str, k: int = TOP_K_RESULTS) -> RAGContext:
        t0 = time.time()

        # ✅ Pas de RAG pour les salutations
        if is_greeting(query):
            return RAGContext(chunks=[], sources=[], retrieval_time=0.0)

        if not vector_store.is_ready():
            return RAGContext(chunks=[], sources=[], retrieval_time=0.0)

        results: List[Tuple[Chunk, float]] = vector_store.search(query, k=k)
        chunks_texts = []
        sources = []
        seen = set()

        for chunk, score in results:
            chunks_texts.append(chunk.text)
            title = get_title(chunk.source_file)
            if title not in seen:
                seen.add(title)
                sources.append(Source(
                    filename=title,
                    page=chunk.page,
                    excerpt="",
                    score=round(score, 4)
                ))

        return RAGContext(
            chunks=chunks_texts,
            sources=sources,
            retrieval_time=round(time.time() - t0, 3)
        )

    def build_prompt(
        self,
        query: str,
        context: RAGContext,
        history: Optional[List[dict]] = None,
        language: str = "fr"
    ) -> str:

        # ✅ Prompt spécial pour les salutations
        if is_greeting(query):
            if language == "ar":
                return f"[INST] أنت IDARATECH مستشار إداري تونسي. المستخدم قال: '{query}'. رد بتحية احترافية ودية واسأله كيف يمكنك مساعدته في إجراءاته الإدارية. [/INST]"
            else:
                return f"[INST] Tu es IDARATECH conseiller administratif tunisien. L'utilisateur dit: '{query}'. Réponds avec une salutation professionnelle et chaleureuse, présente-toi brièvement et demande comment tu peux l'aider. [/INST]"

        # Langue
        if language == "ar":
            lang = "Réponds en arabe uniquement."
        else:
            lang = "Réponds en français uniquement."

        # Contexte RAG
        if context.chunks:
            ctx = "\n\n".join(context.chunks[:2])
            context_block = f"\nInformations officielles :\n{ctx}\n"
        else:
            context_block = "\nUtilise tes connaissances des procédures administratives tunisiennes.\n"

        return f"[INST] Tu es IDARATECH conseiller administratif tunisien. {lang}{context_block}\nQuestion : {query}\nRéponds directement et professionnellement avec les étapes, documents requis, lieu, délai et coût. [/INST]"

    def format_response(self, raw: str) -> str:
        text = raw.strip()
        # Supprimer balises
        for tag in ["[INST]", "[/INST]", "### RÉPONSE", "Réponse :",
                    "Assistant :", "IDARATECH :"]:
            text = text.replace(tag, "").strip()
        # Supprimer hallucinations
        if text.lower().startswith(("utilisateur:", "user:", "assistant:")):
            lines = [l for l in text.split("\n")
                     if not l.lower().startswith(("utilisateur:", "user:", "assistant:"))
                     and len(l.strip()) > 0]
            text = "\n".join(lines).strip()
        return text or "Je n'ai pas pu traiter votre demande."


rag_pipeline = RAGPipeline()