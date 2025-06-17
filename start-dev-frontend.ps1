# Script pour démarrer le frontend en mode développement
# Nécessite que l'environnement backend soit déjà démarré avec start-dev-backend.ps1

Write-Host "🚀 Démarrage du frontend en mode développement..." -ForegroundColor Green

# Vérifier que nous sommes dans le bon répertoire
if (-not (Test-Path "package.json")) {
    Write-Host "❌ Ce script doit être exécuté depuis le dossier frontend" -ForegroundColor Red
    Write-Host "📍 Usage: cd frontend && ../start-dev-frontend.ps1" -ForegroundColor Yellow
    exit 1
}

# Vérifier que l'API backend est accessible
Write-Host "🔍 Vérification de l'API backend..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5041/health" -Method Get -TimeoutSec 3
    Write-Host "✅ API backend opérationnelle: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ API backend non accessible sur http://localhost:5041" -ForegroundColor Red
    Write-Host "🔧 Veuillez d'abord démarrer l'environnement backend avec:" -ForegroundColor Yellow
    Write-Host "   ./start-dev-backend.ps1" -ForegroundColor White
    exit 1
}

# Vérifier que les dépendances sont installées
if (-not (Test-Path "node_modules")) {
    Write-Host "📦 Installation des dépendances..." -ForegroundColor Yellow
    npm install
}

Write-Host ""
Write-Host "🎯 Démarrage du serveur de développement Vite..." -ForegroundColor Green
Write-Host "📍 Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "📍 API: http://localhost:5041" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔥 Hot-reload activé pour un développement rapide!" -ForegroundColor Magenta
Write-Host ""

# Démarrer le serveur de développement
npm run dev
