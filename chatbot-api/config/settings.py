# config/settings.py
import os
from pathlib import Path
from dotenv import load_dotenv

load_dotenv()

BASE_DIR         = Path(__file__).parent.parent
DATA_DIR         = BASE_DIR / "data"
DOCUMENTS_DIR    = DATA_DIR / "documents"
VECTOR_STORE_DIR = DATA_DIR / "vector_store"

DOCUMENTS_DIR.mkdir(parents=True, exist_ok=True)
VECTOR_STORE_DIR.mkdir(parents=True, exist_ok=True)

OLLAMA_BASE_URL  = os.getenv("OLLAMA_BASE_URL", "http://localhost:11434")
OLLAMA_MODEL     = os.getenv("OLLAMA_MODEL", "llama3.2:1b")
OLLAMA_TIMEOUT   = int(os.getenv("OLLAMA_TIMEOUT", "120"))

EMBEDDING_MODEL  = os.getenv(
    "EMBEDDING_MODEL",
    "paraphrase-multilingual-MiniLM-L12-v2"
)

CHUNK_SIZE           = int(os.getenv("CHUNK_SIZE", "500"))
CHUNK_OVERLAP        = int(os.getenv("CHUNK_OVERLAP", "100"))
TOP_K_RESULTS        = int(os.getenv("TOP_K_RESULTS", "3"))
SIMILARITY_THRESHOLD = float(os.getenv("SIMILARITY_THRESHOLD", "0.3"))

API_HOST     = os.getenv("API_HOST", "0.0.0.0")
API_PORT     = int(os.getenv("API_PORT", "8001"))
API_TITLE    = "IDARATECH Chatbot API"
API_VERSION  = "1.0.0"
CORS_ORIGINS = ["http://localhost:4200", "http://localhost:3000", "*"]

DATABASE_URL = os.getenv(
    "DATABASE_URL",
    f"sqlite:///{BASE_DIR}/idaratech_chat.db"
)

# ══ Mots de salutation — pas besoin de RAG ══════════════════
GREETING_WORDS = [
    "bonjour", "bonsoir", "salut", "hello", "hi", "coucou",
    "salam", "ahlan", "marhaba",
    "مرحبا", "السلام", "أهلا", "صباح", "مساء", "سلام",
    "صباح الخير", "مساء الخير"
]

# ══ SYSTEM PROMPT ════════════════════════════════════════════
SYSTEM_PROMPT = """[INST] Tu es IDARATECH, conseiller administratif officiel tunisien.

RÈGLES :
- Réponds DIRECTEMENT, sans répéter la question
- Adapte la langue : fr→français, ar/darija→arabe, en→anglais
- Pour les salutations : réponds chaleureusement et demande comment aider
- Pour les questions admin : donne étapes numérotées + documents + lieu + délai + coût
- NE commence jamais par "Utilisateur:" ou "Assistant:" [/INST]"""