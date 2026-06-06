# ============================================================
# chatbot/api/crud.py
# Opérations CRUD — Database
# ============================================================
from typing import List, Optional
from sqlalchemy.orm import Session
from sqlalchemy import func
from chatbot.api.database import ConversationDB, DocumentDB, FeedbackDB
from chatbot.models.schemas import DocumentInfo, DocumentStatus, StatsResponse
from datetime import datetime


def save_message(db: Session, session_id: str, role: str, content: str):
    msg = ConversationDB(session_id=session_id, role=role, content=content)
    db.add(msg); db.commit()
    return msg.id


def get_session_messages(db: Session, session_id: str) -> List[dict]:
    msgs = db.query(ConversationDB).filter(
        ConversationDB.session_id == session_id
    ).order_by(ConversationDB.created_at).all()
    return [{"role": m.role, "content": m.content,
             "timestamp": m.created_at.isoformat() if m.created_at else None}
            for m in msgs]


def delete_session(db: Session, session_id: str) -> int:
    deleted = db.query(ConversationDB).filter(
        ConversationDB.session_id == session_id
    ).delete()
    db.commit()
    return deleted


def create_document(db: Session, original_name: str, filename: str,
                    file_size: int, category: str) -> int:
    doc = DocumentDB(original_name=original_name, filename=filename,
                     file_size=file_size, category=category)
    db.add(doc); db.commit(); db.refresh(doc)
    return doc.id


def update_document_status(db: Session, doc_id: int, status: str, chunks: int):
    doc = db.query(DocumentDB).filter(DocumentDB.id == doc_id).first()
    if doc:
        doc.status = status
        doc.chunks_count = chunks
        db.commit()


def get_all_documents(db: Session) -> List[DocumentInfo]:
    docs = db.query(DocumentDB).order_by(DocumentDB.created_at.desc()).all()
    return [DocumentInfo(
        id=d.id, filename=d.filename, original_name=d.original_name,
        status=DocumentStatus(d.status), chunks_count=d.chunks_count,
        file_size=d.file_size, created_at=d.created_at
    ) for d in docs]


def get_document(db: Session, doc_id: int) -> Optional[dict]:
    doc = db.query(DocumentDB).filter(DocumentDB.id == doc_id).first()
    if doc:
        return {"id": doc.id, "filename": doc.filename,
                "original_name": doc.original_name}
    return None


def delete_document(db: Session, doc_id: int):
    db.query(DocumentDB).filter(DocumentDB.id == doc_id).delete()
    db.commit()


def save_feedback(db: Session, session_id: str, rating: int, comment: Optional[str]):
    fb = FeedbackDB(session_id=session_id, rating=rating, comment=comment)
    db.add(fb); db.commit()


def get_global_stats(db: Session) -> StatsResponse:
    total_msg   = db.query(ConversationDB).count()
    total_sess  = db.query(func.count(func.distinct(ConversationDB.session_id))).scalar() or 0
    total_docs  = db.query(DocumentDB).count()
    return StatsResponse(
        total_conversations=total_sess,
        total_messages=total_msg,
        total_documents=total_docs,
        avg_response_time=1.5,
        rag_usage_rate=0.85
    )
