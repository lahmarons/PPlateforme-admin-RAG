# ============================================================
# tests/test_chatbot.py
# Tests unitaires — IDARATECH Chatbot
# Tests de l'architecture RAG complète
# ============================================================
import sys
import time
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))


class TestEmbeddingService(unittest.TestCase):
    """Tests du service Sentence Transformers"""

    def setUp(self):
        from chatbot.core.embeddings import EmbeddingService
        self.service = EmbeddingService()

    def test_encode_single_text(self):
        """Un texte → vecteur de la bonne dimension"""
        vec = self.service.encode_single("Comment renouveler ma CIN ?")
        self.assertEqual(len(vec.shape), 1)
        self.assertGreater(vec.shape[0], 0)
        print(f"✅ Embedding dimension: {vec.shape[0]}")

    def test_encode_batch(self):
        """Batch d'encodage"""
        texts = ["CIN", "passeport", "acte de naissance", "شهادة الميلاد"]
        vecs = self.service.encode(texts)
        self.assertEqual(vecs.shape[0], len(texts))
        print(f"✅ Batch encoding: {vecs.shape}")

    def test_similarity(self):
        """Textes similaires → score élevé"""
        v1 = self.service.encode_single("renouveler carte identité")
        v2 = self.service.encode_single("renouvellement CIN")
        v3 = self.service.encode_single("recette cuisine tunisienne")

        sim_related = self.service.similarity(v1, v2)
        sim_unrelated = self.service.similarity(v1, v3)

        print(f"✅ Similarité CIN/CIN: {sim_related:.4f}")
        print(f"✅ Similarité CIN/cuisine: {sim_unrelated:.4f}")
        self.assertGreater(sim_related, sim_unrelated)

    def test_multilingual(self):
        """Support arabe + français"""
        fr = self.service.encode_single("carte d'identité nationale")
        ar = self.service.encode_single("بطاقة الهوية الوطنية")
        score = self.service.similarity(fr, ar)
        print(f"✅ FR↔AR similarité: {score:.4f}")
        self.assertGreater(score, 0.3)  # Modèle multilingue


class TestDocumentProcessor(unittest.TestCase):
    """Tests du pipeline de traitement documentaire"""

    def setUp(self):
        from chatbot.rag.document_processor import DocumentProcessor
        self.processor = DocumentProcessor()

    def test_chunking(self):
        """Découpage en chunks avec overlap"""
        text = "Ceci est un document administratif. " * 50
        chunks = self.processor.split_text(text, chunk_size=200, overlap=50)
        self.assertGreater(len(chunks), 1)
        print(f"✅ {len(chunks)} chunks créés")

    def test_chunk_overlap(self):
        """Les chunks se chevauchent correctement"""
        text = "A " * 200
        chunks = self.processor.split_text(text, chunk_size=100, overlap=30)
        # Les chunks doivent se chevaucher
        if len(chunks) > 1:
            end1 = chunks[0][2]    # char_end du chunk 1
            start2 = chunks[1][1]  # char_start du chunk 2
            self.assertLess(start2, end1)
            print(f"✅ Overlap: chunk1_end={end1}, chunk2_start={start2}")

    def test_clean_text(self):
        """Nettoyage du texte"""
        dirty = "Texte   avec    espaces\n\n\n\n\nmultiples"
        clean = self.processor._clean_text(dirty)
        self.assertNotIn("   ", clean)
        print(f"✅ Texte nettoyé: '{clean[:50]}'")

    def test_supported_formats(self):
        """Validation des formats"""
        self.assertTrue(self.processor.is_supported("doc.pdf"))
        self.assertTrue(self.processor.is_supported("doc.txt"))
        self.assertFalse(self.processor.is_supported("doc.xls"))
        print("✅ Validation formats OK")


class TestVectorStore(unittest.TestCase):
    """Tests du vector store FAISS"""

    def setUp(self):
        from chatbot.core.vector_store import FAISSVectorStore, Chunk
        self.store = FAISSVectorStore()
        self.store.initialize()
        self.Chunk = Chunk

    def test_add_and_search(self):
        """Ajouter des chunks et les retrouver"""
        chunks = [
            self.Chunk(
                id=0,
                text="La CIN est la carte d'identité nationale tunisienne.",
                source_file="test.txt",
                page=1,
                chunk_index=0,
                char_start=0,
                char_end=52
            ),
            self.Chunk(
                id=0,
                text="Le passeport permet de voyager à l'étranger.",
                source_file="test.txt",
                page=1,
                chunk_index=1,
                char_start=52,
                char_end=100
            ),
        ]

        added = self.store.add_chunks(chunks)
        self.assertGreater(added, 0)
        print(f"✅ {added} chunks ajoutés")

        # Recherche
        results = self.store.search("carte identité nationale", k=1)
        self.assertGreater(len(results), 0)
        chunk, score = results[0]
        print(f"✅ Résultat trouvé: score={score:.4f}, texte='{chunk.text[:40]}...'")
        self.assertGreater(score, 0.3)

    def test_stats(self):
        """Statistiques de l'index"""
        stats = self.store.get_stats()
        self.assertIn("total_vectors", stats)
        self.assertIn("status", stats)
        print(f"✅ Stats: {stats}")


class TestRAGPipeline(unittest.TestCase):
    """Tests du pipeline RAG complet"""

    def setUp(self):
        from chatbot.rag.rag_pipeline import RAGPipeline
        self.pipeline = RAGPipeline()

    def test_build_prompt_with_context(self):
        """Construction du prompt avec contexte"""
        class FakeContext:
            chunks = ["Texte du document 1", "Texte du document 2"]
            sources = []

        prompt = self.pipeline.build_prompt(
            query="Comment renouveler la CIN ?",
            context=FakeContext(),
            language="fr"
        )
        self.assertIn("CIN", prompt)
        self.assertIn("Texte du document", prompt)
        print(f"✅ Prompt construit: {len(prompt)} caractères")

    def test_build_prompt_without_context(self):
        """Prompt sans contexte RAG"""
        class EmptyContext:
            chunks = []
            sources = []

        prompt = self.pipeline.build_prompt(
            query="Comment renouveler la CIN ?",
            context=EmptyContext(),
            language="fr"
        )
        self.assertIn("connaissances", prompt)
        print("✅ Prompt sans contexte OK")

    def test_format_response(self):
        """Nettoyage de la réponse LLM"""
        raw = "### RÉPONSE\nVoici la réponse."
        formatted = self.pipeline.format_response(raw)
        self.assertNotIn("### RÉPONSE", formatted)
        print(f"✅ Réponse formatée: '{formatted}'")


class TestOllamaService(unittest.TestCase):
    """Tests du service Ollama (sans connexion requise)"""

    def setUp(self):
        from chatbot.core.llm_service import OllamaLLMService
        self.service = OllamaLLMService()

    def test_fallback_cin(self):
        """Fallback pour question CIN"""
        result = self.service._fallback("Comment renouveler ma CIN ?")
        self.assertIn("response", result)
        self.assertIn("CIN", result["response"])
        print(f"✅ Fallback CIN: '{result['response'][:50]}...'")

    def test_fallback_passeport(self):
        """Fallback pour question passeport"""
        result = self.service._fallback("Je veux faire un passeport")
        self.assertIn("response", result)
        self.assertIn("passeport", result["response"].lower())
        print(f"✅ Fallback passeport OK")

    def test_availability_check(self):
        """Vérification disponibilité (ne plante pas)"""
        available = self.service.is_available()
        print(f"✅ Ollama disponible: {available}")
        self.assertIsInstance(available, bool)


if __name__ == "__main__":
    print("\n" + "="*60)
    print("  IDARATECH Chatbot — Tests Unitaires")
    print("="*60 + "\n")

    loader = unittest.TestLoader()
    suite = unittest.TestSuite()

    suite.addTests(loader.loadTestsFromTestCase(TestEmbeddingService))
    suite.addTests(loader.loadTestsFromTestCase(TestDocumentProcessor))
    suite.addTests(loader.loadTestsFromTestCase(TestVectorStore))
    suite.addTests(loader.loadTestsFromTestCase(TestRAGPipeline))
    suite.addTests(loader.loadTestsFromTestCase(TestOllamaService))

    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(suite)

    print("\n" + "="*60)
    if result.wasSuccessful():
        print(f"  ✅ Tous les tests passés ({result.testsRun} tests)")
    else:
        print(f"  ❌ {len(result.failures)} échecs, {len(result.errors)} erreurs")
    print("="*60)
