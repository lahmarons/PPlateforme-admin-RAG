# ============================================================
# chatbot/api/database.py
# Base de données SQLite — Sessions et documents
# ============================================================
from sqlalchemy import create_engine, Column, Integer, String, Text, DateTime, Float, Boolean
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
from sqlalchemy.sql import func
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))
from config.settings import DATABASE_URL

engine = create_engine(DATABASE_URL, connect_args={"check_same_thread": False})
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()


class ConversationDB(Base):
    __tablename__ = "conversations"
    id         = Column(Integer, primary_key=True, index=True)
    session_id = Column(String(100), index=True)
    role       = Column(String(20))
    content    = Column(Text)
    created_at = Column(DateTime(timezone=True), server_default=func.now())


class DocumentDB(Base):
    __tablename__ = "documents"
    id            = Column(Integer, primary_key=True, index=True)
    filename      = Column(String(255))
    original_name = Column(String(255))
    file_size     = Column(Integer, default=0)
    category      = Column(String(100), default="general")
    status        = Column(String(20), default="pending")
    chunks_count  = Column(Integer, default=0)
    created_at    = Column(DateTime(timezone=True), server_default=func.now())


class FeedbackDB(Base):
    __tablename__ = "feedbacks"
    id         = Column(Integer, primary_key=True, index=True)
    session_id = Column(String(100))
    rating     = Column(Integer)
    comment    = Column(Text)
    created_at = Column(DateTime(timezone=True), server_default=func.now())


def create_tables():
    Base.metadata.create_all(bind=engine)


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
