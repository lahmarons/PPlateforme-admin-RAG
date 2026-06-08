# ============================================================
# tests/test_api.py
# Tests d'intégration de l'API — IDARATECH Chatbot
# ============================================================
import sys
import unittest
from pathlib import Path
from unittest.mock import MagicMock, patch

# Configuration du PYTHONPATH
sys.path.insert(0, str(Path(__file__).parent.parent))

# 2. Imports FastAPI
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from chatbot.api.main import app
from chatbot.api.database import Base, get_db

# 3. Setup de la base de données de test en mémoire
from sqlalchemy.pool import StaticPool
engine = create_engine(
    "sqlite://",
    connect_args={"check_same_thread": False},
    poolclass=StaticPool
)
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

def override_get_db():
    db = TestingSessionLocal()
    try:
        yield db
    finally:
        db.close()

app.dependency_overrides[get_db] = override_get_db


class TestChatbotAPI(unittest.TestCase):
    """Tests d'intégration pour les endpoints de l'API FastAPI"""

    @classmethod
    def setUpClass(cls):
        # Configuration des patchers dynamiques pour isoler le test sans polluer les autres suites
        cls.patchers = [
            patch('chatbot.core.embeddings.embedding_service.load'),
            patch('chatbot.core.embeddings.embedding_service.is_ready', return_value=True),
            patch('chatbot.core.embeddings.embedding_service.get_dimension', return_value=384),
            patch('chatbot.core.embeddings.embedding_service.encode', return_value=None),
            patch('chatbot.core.embeddings.embedding_service.encode_single', return_value=None),
            
            patch('chatbot.core.vector_store.vector_store.initialize'),
            patch('chatbot.core.vector_store.vector_store.is_ready', return_value=True),
            patch('chatbot.core.vector_store.vector_store.get_stats', return_value={
                "total_vectors": 10,
                "total_chunks": 10,
                "dimension": 384,
                "index_size_mb": 0.05,
                "unique_sources": 1,
                "status": "ready"
            }),
            patch('chatbot.core.vector_store.vector_store.add_chunks', return_value=2),
            patch('chatbot.core.vector_store.vector_store.remove_by_source', return_value=2),
            patch('chatbot.core.vector_store.vector_store.search', return_value=[]),
            
            patch('chatbot.core.llm_service.llm_service.is_available', return_value=True),
            patch('chatbot.core.llm_service.llm_service.list_models', return_value=["llama3.2:1b"]),
            patch('chatbot.core.llm_service.llm_service.generate', return_value={
                "response": "Ceci est une réponse simulée pour la CIN.",
                "model": "llama3.2:1b",
                "eval_count": 42
            })
        ]

        # Démarrage des patchers
        for p in cls.patchers:
            p.start()

        # Création des tables dans la DB de test en mémoire
        Base.metadata.create_all(bind=engine)
        cls.client = TestClient(app)

    @classmethod
    def tearDownClass(cls):
        # Arrêt des patchers
        for p in cls.patchers:
            p.stop()
            
        # Suppression des tables après les tests
        Base.metadata.drop_all(bind=engine)

    def test_health_endpoint(self):
        """Vérifie que la route /health renvoie le bon format"""
        response = self.client.get("/health")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["status"], "healthy")
        self.assertTrue(data["ollama_available"])
        self.assertEqual(data["total_documents"], 1)

    def test_chat_endpoint(self):
        """Vérifie le traitement de la route /chat"""
        payload = {
            "message": "Comment renouveler ma CIN ?",
            "session_id": "test_session_123",
            "language": "fr",
            "use_rag": True,
            "history": []
        }
        response = self.client.post("/chat", json=payload)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn("answer", data)
        self.assertEqual(data["session_id"], "test_session_123")
        self.assertEqual(data["model_used"], "llama3.2:1b")
        self.assertEqual(data["tokens_used"], 42)

    def test_get_history_endpoint(self):
        """Vérifie la route /history/{session_id} et sa suppression"""
        session_id = "test_session_history"
        
        # 1. Récupération de l'historique initial (vide)
        response = self.client.get(f"/history/{session_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["session_id"], session_id)
        self.assertEqual(len(data["messages"]), 0)

        # 2. Ajout de messages en simulant un chat
        chat_payload = {
            "message": "Bonjour",
            "session_id": session_id,
            "language": "fr",
            "use_rag": False,
            "history": []
        }
        self.client.post("/chat", json=chat_payload)

        # 3. Récupération de l'historique mis à jour (devrait contenir user + assistant)
        response = self.client.get(f"/history/{session_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(len(data["messages"]), 2)
        self.assertEqual(data["messages"][0]["role"], "user")
        self.assertEqual(data["messages"][0]["content"], "Bonjour")

        # 4. Suppression de l'historique
        delete_response = self.client.delete(f"/history/{session_id}")
        self.assertEqual(delete_response.status_code, 200)
        
        # 5. Vérification que l'historique est de nouveau vide
        response = self.client.get(f"/history/{session_id}")
        self.assertEqual(len(response.json()["messages"]), 0)

    def test_document_list_and_upload(self):
        """Vérifie la liste et l'upload de documents (avec simulation de FAISS)"""
        # 1. Vérification de la liste de documents initiale (vide)
        response = self.client.get("/documents")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["total"], 0)

        # 2. Upload d'un faux fichier document
        file_content = b"Ceci est un document de test pour l'indexation."
        files = {
            "file": ("document_test.txt", file_content, "text/plain")
        }
        upload_response = self.client.post(
            "/documents/upload?category=identity",
            files=files
        )
        self.assertEqual(upload_response.status_code, 200)
        upload_data = upload_response.json()
        self.assertTrue(upload_data["success"])
        self.assertEqual(upload_data["filename"], "document_test.txt")
        doc_id = upload_data["document_id"]

        # 3. Vérification de la liste contenant le document
        response = self.client.get("/documents")
        self.assertEqual(response.json()["total"], 1)
        self.assertEqual(response.json()["documents"][0]["original_name"], "document_test.txt")

        # 4. Suppression du document
        delete_response = self.client.delete(f"/documents/{doc_id}")
        self.assertEqual(delete_response.status_code, 200)

        # 5. Vérification finale que la liste est vide
        response = self.client.get("/documents")
        self.assertEqual(response.json()["total"], 0)

    def test_index_stats_and_reset(self):
        """Vérifie les routes d'administration de l'index FAISS"""
        # Stats
        response = self.client.get("/index/stats")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["total_vectors"], 10)
        self.assertEqual(data["status"], "ready")

        # Reset
        response = self.client.post("/index/reset")
        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.json()["success"])

    def test_feedback_endpoint(self):
        """Vérifie la soumission de feedback"""
        payload = {
            "session_id": "test_session_feedback",
            "rating": 5,
            "comment": "Excellent travail !"
        }
        response = self.client.post("/feedback", json=payload)
        self.assertEqual(response.status_code, 200)
        self.assertTrue(response.json()["success"])

    def test_admin_stats(self):
        """Vérifie la route d'administration des statistiques globales"""
        response = self.client.get("/admin/stats")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn("total_conversations", data)
        self.assertIn("total_messages", data)
        self.assertIn("total_documents", data)
        self.assertIn("avg_response_time", data)
        self.assertIn("rag_usage_rate", data)

    def test_chat_endpoint_validation_error(self):
        """Vérifie qu'un message vide ou manquant génère une erreur 422"""
        payload = {
            "message": "",
            "session_id": "test_session_empty",
            "language": "fr",
            "use_rag": True,
            "history": []
        }
        response = self.client.post("/chat", json=payload)
        self.assertEqual(response.status_code, 422)

    def test_feedback_endpoint_validation_error(self):
        """Vérifie qu'une note invalide (hors de [1, 5]) renvoie une erreur 422"""
        payload = {
            "session_id": "test_session_feedback",
            "rating": 6,
            "comment": "Super"
        }
        response = self.client.post("/feedback", json=payload)
        self.assertEqual(response.status_code, 422)


if __name__ == "__main__":
    unittest.main()
