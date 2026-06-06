# ============================================================
# chatbot/api/main.py
# FastAPI Application — IDARATECH Chatbot
# Conforme Chapitre 4.4.1 : "FastAPI : framework Python utilisé
#  pour développer les API REST du chatbot"
# ============================================================
import os
import uuid
import time
import shutil
from pathlib import Path

from fastapi import FastAPI, Depends, HTTPException, File, UploadFile, Query
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.orm import Session

import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from config.settings import (
    API_TITLE, API_VERSION, CORS_ORIGINS,
    DOCUMENTS_DIR, OLLAMA_MODEL
)
from chatbot.models.schemas import (
    ChatRequest, ChatResponse, Source,
    DocumentUploadResponse, DocumentInfo, DocumentList,
    HealthResponse, StatsResponse, FeedbackRequest,
    IndexStats
)
from chatbot.core.embeddings  import embedding_service
from chatbot.core.vector_store import vector_store
from chatbot.core.llm_service  import llm_service
from chatbot.rag.rag_pipeline   import rag_pipeline
from chatbot.rag.document_processor import document_processor
from chatbot.api.database import get_db, create_tables, SessionLocal
from chatbot.api import crud


# ── App Init ─────────────────────────────────────────────────

app = FastAPI(
    title=API_TITLE,
    description="""
## IDARATECH Chatbot API

Architecture RAG complète :
- **Sentence Transformers** → Embeddings multilingues (arabe + français)
- **FAISS** → Recherche sémantique vectorielle
- **Ollama/Mistral 7B** → Génération de réponses intelligentes
- **FastAPI** → API REST haute performance

Conforme au Chapitre 4 du rapport PFA IDARATECH.
    """,
    version=API_VERSION,
    docs_url="/docs",
    redoc_url="/redoc"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=CORS_ORIGINS,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── Startup ───────────────────────────────────────────────────

@app.on_event("startup")
async def startup():
    print("\n" + "="*50)
    print("  IDARATECH Chatbot API — Démarrage")
    print("="*50)

    # Base de données
    create_tables()
    print("✅ Base de données initialisée")

    # Embeddings
    embedding_service.load()
    print(f"✅ Sentence Transformers prêt (dim={embedding_service.get_dimension()})")

    # FAISS
    vector_store.initialize()
    stats = vector_store.get_stats()
    print(f"✅ FAISS Index: {stats['total_vectors']} vecteurs")

    # Ollama
    available = llm_service.is_available()
    if available:
        models = llm_service.list_models()
        print(f"✅ Ollama disponible — Modèles: {models}")
    else:
        print("⚠️  Ollama non disponible — Mode fallback activé")
        print("   → Lancez: ollama serve && ollama pull mistral")

    print("="*50 + "\n")


# ── HEALTH ────────────────────────────────────────────────────

@app.get("/health", response_model=HealthResponse, tags=["System"])
async def health_check(db: Session = Depends(get_db)):
    """Statut complet du système"""
    faiss_stats = vector_store.get_stats()

    return HealthResponse(
        status="healthy",
        version=API_VERSION,
        ollama_available=llm_service.is_available(),
        ollama_model=OLLAMA_MODEL,
        faiss_index_loaded=vector_store.is_ready(),
        total_documents=faiss_stats.get("unique_sources", 0),
        embedding_model=str(embedding_service._model.__class__.__name__
                            if embedding_service.is_ready() else "not_loaded")
    )


# ── CHAT — Route principale ───────────────────────────────────

@app.post("/chat", response_model=ChatResponse, tags=["Chat"])
async def chat(request: ChatRequest, db: Session = Depends(get_db)):
    """
    Endpoint principal du chatbot RAG.
    
    Pipeline (Chapitre 4.5) :
    1. Recevoir la question utilisateur
    2. Générer l'embedding de la question
    3. Recherche FAISS → Top-K chunks pertinents
    4. Construire le prompt : System + Contexte + Historique + Question
    5. Ollama/Mistral génère la réponse
    6. Retourner réponse + sources au frontend Angular
    """
    t_start = time.time()
    print(f"\n[CHAT] 📥 Message reçu: '{request.message}'")

    # 1. Sauvegarder le message utilisateur
    crud.save_message(db, request.session_id, "user", request.message)

    # 2 & 3. RETRIEVAL — Recherche FAISS
    t_retrieval_start = time.time()
    rag_context = None
    sources = []
    rag_used = False

    if request.use_rag and vector_store.is_ready():
        rag_context = rag_pipeline.retrieve(request.message)
        sources = rag_context.sources
        rag_used = len(rag_context.chunks) > 0
    
    print(f"[CHAT] 🔍 Recherche FAISS effectuée en {time.time() - t_retrieval_start:.4f}s")

    # 4. Construire le prompt
    history = [{"role": m.role, "content": m.content}
               for m in (request.history or [])]

    prompt = rag_pipeline.build_prompt(
        query=request.message,
        context=rag_context or type("ctx", (), {"chunks": [], "sources": []})(),
        history=history,
        language=request.language.value
    )

    # 5. GENERATION — Ollama/Mistral
    t_gen_start = time.time()
    llm_result = llm_service.generate(prompt)
    raw_answer = llm_result.get("response", "")
    answer = rag_pipeline.format_response(raw_answer)
    print(f"[CHAT] 🤖 Génération LLM effectuée en {time.time() - t_gen_start:.4f}s")

    processing_time = round(time.time() - t_start, 2)
    print(f"[CHAT] ⚡ Temps de traitement API total: {processing_time}s\n")

    # 6. Sauvegarder la réponse
    crud.save_message(db, request.session_id, "assistant", answer)

    return ChatResponse(
        answer=answer,
        session_id=request.session_id,
        sources=sources,
        language_detected=request.language.value,
        processing_time=processing_time,
        model_used=llm_result.get("model", OLLAMA_MODEL),
        rag_used=rag_used,
        tokens_used=llm_result.get("eval_count")
    )


# ── DOCUMENTS ─────────────────────────────────────────────────

@app.post("/documents/upload", response_model=DocumentUploadResponse, tags=["Documents"])
async def upload_document(
    file: UploadFile = File(...),
    category: str = Query("general"),
    db: Session = Depends(get_db)
):
    """
    Upload et indexation d'un document dans FAISS.
    
    Pipeline (Chapitre 4.3.2 — Data Indexing) :
    PDF Upload → Text Extraction → Chunking → Embedding → FAISS
    """
    # Validation format
    if not document_processor.is_supported(file.filename):
        raise HTTPException(
            status_code=400,
            detail=f"Format non supporté. Formats acceptés: {document_processor.SUPPORTED_FORMATS}"
        )

    # Sauvegarder le fichier
    safe_name = f"{uuid.uuid4()}_{file.filename}"
    file_path = DOCUMENTS_DIR / safe_name

    with open(file_path, "wb") as f:
        content = await file.read()
        f.write(content)

    # Sauvegarder en DB
    doc_id = crud.create_document(db, file.filename, safe_name, len(content), category)

    # Indexer dans FAISS
    try:
        chunks_count = document_processor.process_file(file_path, file.filename)
        indexed = chunks_count > 0
        crud.update_document_status(db, doc_id, "indexed", chunks_count)
        message = f"✅ Document indexé : {chunks_count} chunks créés"
    except Exception as e:
        indexed = False
        chunks_count = 0
        crud.update_document_status(db, doc_id, "failed", 0)
        message = f"⚠️ Indexation échouée: {str(e)}"

    return DocumentUploadResponse(
        success=indexed,
        document_id=doc_id,
        filename=file.filename,
        chunks_created=chunks_count,
        indexed=indexed,
        message=message
    )


@app.get("/documents", response_model=DocumentList, tags=["Documents"])
async def list_documents(db: Session = Depends(get_db)):
    """Liste tous les documents indexés"""
    docs = crud.get_all_documents(db)
    return DocumentList(documents=docs, total=len(docs))


@app.delete("/documents/{doc_id}", tags=["Documents"])
async def delete_document(doc_id: int, db: Session = Depends(get_db)):
    """Supprime un document et ses chunks FAISS"""
    doc = crud.get_document(db, doc_id)
    if not doc:
        raise HTTPException(status_code=404, detail="Document non trouvé")

    # Supprimer les chunks FAISS
    removed = vector_store.remove_by_source(doc["original_name"])

    # Supprimer le fichier
    file_path = DOCUMENTS_DIR / doc["filename"]
    if file_path.exists():
        file_path.unlink()

    # Supprimer de la DB
    crud.delete_document(db, doc_id)

    return {
        "success": True,
        "document_id": doc_id,
        "chunks_removed": removed
    }


# ── FAISS INDEX ───────────────────────────────────────────────

@app.get("/index/stats", response_model=IndexStats, tags=["Index"])
async def index_stats():
    """Statistiques de l'index FAISS"""
    stats = vector_store.get_stats()
    return IndexStats(
        total_vectors=stats.get("total_vectors", 0),
        total_documents=stats.get("unique_sources", 0),
        embedding_model=str(embedding_service._model.__class__.__name__
                            if embedding_service.is_ready() else "not_loaded"),
        index_size_mb=stats.get("index_size_mb", 0.0),
        status=stats.get("status", "unknown")
    )


@app.post("/index/reset", tags=["Index"])
async def reset_index():
    """Réinitialise complètement l'index FAISS (⚠️ irréversible)"""
    vector_store._create_empty_index()
    vector_store.save()
    return {"success": True, "message": "Index FAISS réinitialisé"}


# ── HISTORY ───────────────────────────────────────────────────

@app.get("/history/{session_id}", tags=["Chat"])
async def get_history(session_id: str, db: Session = Depends(get_db)):
    """Historique d'une session de chat"""
    messages = crud.get_session_messages(db, session_id)
    return {"session_id": session_id, "messages": messages}


@app.delete("/history/{session_id}", tags=["Chat"])
async def clear_history(session_id: str, db: Session = Depends(get_db)):
    """Efface l'historique d'une session"""
    deleted = crud.delete_session(db, session_id)
    return {"success": True, "deleted_messages": deleted}


# ── FEEDBACK ──────────────────────────────────────────────────

@app.post("/feedback", tags=["Chat"])
async def submit_feedback(req: FeedbackRequest, db: Session = Depends(get_db)):
    """Enregistre le feedback utilisateur"""
    crud.save_feedback(db, req.session_id, req.rating, req.comment)
    return {"success": True}


# ── STATS ADMIN ───────────────────────────────────────────────

@app.get("/admin/stats", response_model=StatsResponse, tags=["Admin"])
async def admin_stats(db: Session = Depends(get_db)):
    """Statistiques globales pour le dashboard admin"""
    return crud.get_global_stats(db)


# ── Main ──────────────────────────────────────────────────────

if __name__ == "__main__":
    import uvicorn
    from config.settings import API_HOST, API_PORT
    uvicorn.run("chatbot.api.main:app", host=API_HOST, port=API_PORT, reload=True)
