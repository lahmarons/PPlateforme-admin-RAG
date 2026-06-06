# ============================================================
# chatbot/models/schemas.py
# Schémas Pydantic — IDARATECH Chatbot
# ============================================================
from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime
from enum import Enum


# ── Enums ────────────────────────────────────────────────────

class Language(str, Enum):
    FR = "fr"
    AR = "ar"
    AUTO = "auto"

class DocumentStatus(str, Enum):
    PENDING  = "pending"
    INDEXED  = "indexed"
    FAILED   = "failed"

class MessageRole(str, Enum):
    USER      = "user"
    ASSISTANT = "assistant"
    SYSTEM    = "system"


# ── Chat Schemas ──────────────────────────────────────────────

class ChatMessage(BaseModel):
    role: MessageRole
    content: str
    timestamp: Optional[datetime] = None


class ChatRequest(BaseModel):
    """Requête entrante du frontend Angular"""
    message: str = Field(..., min_length=1, max_length=2000,
                         description="Question de l'utilisateur")
    session_id: str = Field(..., description="Identifiant de session unique")
    language: Language = Field(Language.AUTO, description="Langue de réponse")
    history: Optional[List[ChatMessage]] = Field(
        default=[], description="Historique de la conversation"
    )
    use_rag: bool = Field(True, description="Activer la recherche RAG")


class Source(BaseModel):
    """Source documentaire utilisée par le RAG"""
    filename: str
    page: Optional[int] = None
    excerpt: str
    score: float


class ChatResponse(BaseModel):
    """Réponse renvoyée au frontend Angular"""
    answer: str
    session_id: str
    sources: List[Source] = []
    language_detected: str = "fr"
    processing_time: float = 0.0
    model_used: str = "mistral"
    rag_used: bool = False
    tokens_used: Optional[int] = None


# ── Document Schemas ──────────────────────────────────────────

class DocumentUploadResponse(BaseModel):
    success: bool
    document_id: int
    filename: str
    chunks_created: int
    indexed: bool
    message: str


class DocumentInfo(BaseModel):
    id: int
    filename: str
    original_name: str
    status: DocumentStatus
    chunks_count: int
    file_size: int
    created_at: datetime


class DocumentList(BaseModel):
    documents: List[DocumentInfo]
    total: int


# ── RAG Schemas ───────────────────────────────────────────────

class RAGContext(BaseModel):
    """Contexte récupéré par le RAG"""
    chunks: List[str]
    sources: List[Source]
    query_embedding_time: float = 0.0
    retrieval_time: float = 0.0


class IndexStats(BaseModel):
    """Statistiques de l'index FAISS"""
    total_vectors: int
    total_documents: int
    embedding_model: str
    index_size_mb: float
    status: str


# ── System Schemas ────────────────────────────────────────────

class HealthResponse(BaseModel):
    status: str
    version: str
    ollama_available: bool
    ollama_model: str
    faiss_index_loaded: bool
    total_documents: int
    embedding_model: str


class StatsResponse(BaseModel):
    total_conversations: int
    total_messages: int
    total_documents: int
    avg_response_time: float
    rag_usage_rate: float


class FeedbackRequest(BaseModel):
    session_id: str
    message_id: Optional[int] = None
    rating: int = Field(..., ge=1, le=5)
    comment: Optional[str] = None
