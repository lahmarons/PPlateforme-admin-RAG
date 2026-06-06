"""
backend/seed_data.py
Script d'indexation automatique des données tunisiennes dans FAISS.
Lance ce script UNE SEULE FOIS pour peupler la base vectorielle.

Usage : python seed_data.py
"""
import sys
import time
from pathlib import Path

# Ajouter le répertoire parent au path
sys.path.insert(0, str(Path(__file__).parent))

DATA_DIR = Path(__file__).parent / "data" / "documents"


def seed():
    print("\n" + "="*60)
    print("  IDARATECH — Indexation des données tunisiennes")
    print("="*60)

    # Vérifier que les documents existent
    docs = list(DATA_DIR.glob("*.txt")) + list(DATA_DIR.glob("*.pdf"))

    if not docs:
        print(f"\n❌ Aucun document trouvé dans : {DATA_DIR}")
        print("   Copiez les fichiers .txt dans ce dossier d'abord.")
        return

    print(f"\n📁 {len(docs)} documents trouvés :")
    for d in docs:
        print(f"   • {d.name}")

    # Importer les services
    try:
        from chatbot.core.embeddings import embedding_service
        from chatbot.core.vector_store import vector_store
        from chatbot.rag.document_processor import document_processor

        # Charger le modèle d'embeddings
        print("\n⏳ Chargement du modèle Sentence Transformers...")
        embedding_service.load()
        if not embedding_service.is_ready():
            print("❌ Modèle non disponible — vérifiez sentence-transformers")
            return
        print(f"✅ Modèle prêt (dim={embedding_service.get_dimension()})")

        # Initialiser FAISS
        vector_store.initialize()
        print(f"✅ FAISS initialisé")

        # Indexer chaque document
        total_chunks = 0
        print("\n📊 Indexation en cours...")

        for doc_path in docs:
            print(f"\n  → {doc_path.name}")
            try:
                chunks = document_processor.process_file(doc_path, doc_path.name)
                total_chunks += chunks
                print(f"     ✅ {chunks} chunks créés")
            except Exception as e:
                print(f"     ❌ Erreur: {e}")

        # Résultat final
        stats = vector_store.get_stats()
        print("\n" + "="*60)
        print(f"  ✅ Indexation terminée !")
        print(f"  📊 Total chunks : {total_chunks}")
        print(f"  📊 Vecteurs FAISS : {stats['total_vectors']}")
        print(f"  📊 Sources : {stats.get('unique_sources', 0)}")
        print("="*60)
        print("\nLe chatbot peut maintenant répondre avec les données tunisiennes.")
        print("Lancez : python -m uvicorn chatbot.api.main:app --port 8001")

    except ImportError as e:
        print(f"\n❌ Import error: {e}")
        print("   Installez d'abord : pip install -r requirements.txt")
    except Exception as e:
        print(f"\n❌ Erreur: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    seed()