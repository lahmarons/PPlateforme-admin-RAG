# chatbot/core/llm_service.py — Version professionnelle
import time
import httpx
from typing import Optional, List
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).parent.parent.parent))
from config.settings import OLLAMA_BASE_URL, OLLAMA_MODEL, OLLAMA_TIMEOUT


class OllamaLLMService:
    def __init__(self):
        self.base_url = OLLAMA_BASE_URL
        self.model    = OLLAMA_MODEL
        self.timeout  = OLLAMA_TIMEOUT
        self._available: Optional[bool] = None

    def is_available(self) -> bool:
        if self._available is True:
            return True
        try:
            with httpx.Client(timeout=3.0) as client:
                resp = client.get(f"{self.base_url}/api/tags")
                if resp.status_code == 200:
                    self._available = True
                    return True
        except Exception:
            pass
        return False

    def list_models(self) -> List[str]:
        try:
            with httpx.Client(timeout=5.0) as client:
                resp = client.get(f"{self.base_url}/api/tags")
                if resp.status_code == 200:
                    return [m["name"] for m in resp.json().get("models", [])]
        except Exception:
            pass
        return []

    def generate(self, prompt: str, temperature: float = 0.1, max_tokens: int = 800) -> dict:
        """
        Génère une réponse via Ollama.
        temperature=0.1 pour des réponses plus déterministes et professionnelles.
        """
        if not self.is_available():
            return self._fallback(prompt)

        try:
            with httpx.Client(timeout=self.timeout) as client:
                resp = client.post(
                    f"{self.base_url}/api/generate",
                    json={
                        "model":  self.model,
                        "prompt": prompt,
                        "stream": False,
                        "options": {
                            "temperature":    temperature,
                            "num_predict":    max_tokens,
                            "top_p":          0.9,
                            "repeat_penalty": 1.15,
                            "stop": [
                                "[INST]", "Utilisateur:",
                                "User:", "\n\nQuestion :"
                            ]
                        }
                    },
                    timeout=self.timeout
                )
                if resp.status_code == 200:
                    data = resp.json()
                    return {
                        "response":   data.get("response", ""),
                        "model":      data.get("model", self.model),
                        "eval_count": data.get("eval_count", 0),
                        "success":    True
                    }
                return self._fallback(prompt, f"HTTP {resp.status_code}")
        except httpx.TimeoutException:
            return self._fallback(prompt, "Timeout")
        except Exception as e:
            return self._fallback(prompt, str(e))

    def _fallback(self, prompt: str, error: str = "") -> dict:
        """Réponses de secours professionnelles quand Ollama n'est pas disponible."""
        p = prompt.lower()

        if any(w in p for w in ["cin", "carte", "identité", "هوية", "بطاقة"]):
            answer = (
                "**Renouvellement de la CIN**\n\n"
                "📋 Documents requis :\n"
                "1. Ancienne CIN\n"
                "2. Acte de naissance récent (moins de 3 mois)\n"
                "3. 2 photos d'identité fond blanc\n"
                "4. Timbre fiscal : 10 DT\n\n"
                "📍 Lieu : Bureau de police ou Garde nationale\n"
                "⏱️ Délai : 15 jours ouvrables\n"
                "🌐 Suivi : www.cinc.tn"
            )
        elif any(w in p for w in ["passeport", "جواز", "باسبور"]):
            answer = (
                "**Demande de Passeport**\n\n"
                "📋 Documents requis :\n"
                "1. CIN valide\n"
                "2. Acte de naissance récent\n"
                "3. 2 photos fond blanc (biométriques)\n"
                "4. Timbre fiscal : 90 DT (ordinaire) / 160 DT (urgent)\n\n"
                "📍 Lieu : Ministère de l'Intérieur ou délégation\n"
                "⏱️ Délai : 15 à 30 jours / Urgent : 72h\n"
                "🌐 www.interieur.gov.tn"
            )
        elif any(w in p for w in ["cnss", "كنص", "sécurité sociale"]):
            answer = (
                "**Affiliation à la CNSS**\n\n"
                "Pour les salariés : l'employeur effectue l'affiliation automatiquement.\n\n"
                "Pour les indépendants :\n"
                "1. Se rendre au bureau CNSS le plus proche\n"
                "2. Apporter : CIN + patente ou registre de commerce\n"
                "3. Remplir le formulaire d'affiliation\n\n"
                "📞 Contact : 71 848 000\n"
                "🌐 www.cnss.nat.tn"
            )
        elif any(w in p for w in ["bonjour", "salam", "مرحبا", "صباح", "سلام", "أهلا"]):
            answer = (
                "Bonjour ! Je suis IDARATECH, votre conseiller administratif tunisien. 😊\n\n"
                "Je suis là pour vous aider dans toutes vos démarches administratives :\n"
                "🪪 Documents d'identité\n"
                "🛂 Passeport\n"
                "🏥 CNSS / CNAM\n"
                "🎓 Université\n"
                "💼 Emploi\n\n"
                "Comment puis-je vous aider aujourd'hui ?"
            )
        else:
            answer = (
                "Je suis IDARATECH, votre conseiller administratif tunisien.\n\n"
                "Je peux vous aider pour :\n"
                "• Documents d'identité (CIN, passeport, permis)\n"
                "• Sécurité sociale (CNSS, CNAM)\n"
                "• Enseignement supérieur\n"
                "• État civil et services municipaux\n"
                "• Emploi et création d'entreprise\n\n"
                "Quelle est votre demande ?"
            )

        return {"response": answer, "model": "fallback", "success": False, "error": error}


llm_service = OllamaLLMService()