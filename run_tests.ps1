# ============================================================
# run_tests.ps1
# Lanceur de tests unifié — IDARATECH
# ============================================================
$ErrorActionPreference = "Stop"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Lancement des tests de l'application IDARATECH" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# ── 1. TESTS PYTHON (CHATBOT API) ──────────────────────────────────
Write-Host "`n[1/3] Exécution des tests Python (Chatbot API)..." -ForegroundColor Yellow
$env:PYTHONIOENCODING = "utf-8"

cd chatbot-api
try {
    # Exécution des tests RAG et API
    venv\Scripts\python -m unittest tests/test_chatbot.py tests/test_api.py
    Write-Host "`n✅ Tests Python passés avec succès !" -ForegroundColor Green
} catch {
    Write-Host "`n❌ Échec des tests Python !" -ForegroundColor Red
    cd ..
    exit 1
}
cd ..

# ── 2. TESTS .NET (DOMAIN & APPLICATION) ──────────────────────────
Write-Host "`n[2/3] Exécution des tests .NET Domain..." -ForegroundColor Yellow
try {
    dotnet test tests/Domain.UnitTests/Domain.UnitTests.csproj
    Write-Host "✅ Tests .NET Domain passés avec succès !" -ForegroundColor Green
} catch {
    Write-Host "❌ Échec des tests .NET Domain !" -ForegroundColor Red
    exit 1
}

Write-Host "`n[3/3] Exécution des tests .NET Application..." -ForegroundColor Yellow
try {
    dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
    Write-Host "✅ Tests .NET Application passés avec succès !" -ForegroundColor Green
} catch {
    Write-Host "❌ Échec des tests .NET Application !" -ForegroundColor Red
    exit 1
}

Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  ✅ Tous les tests (Python & .NET) ont été passés !" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
