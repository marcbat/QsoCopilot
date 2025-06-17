# Script pour démarrer l'environnement de développement backend (MongoDB + API)
# Le frontend sera lancé séparément avec npm run dev

Write-Host "🚀 Démarrage de l'environnement de développement backend..." -ForegroundColor Green

# Arrêter les conteneurs existants s'ils existent
Write-Host "📦 Arrêt des conteneurs existants..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml down 2>$null

# Démarrer les services backend
Write-Host "🔧 Démarrage de MongoDB et de l'API..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml up -d

# Attendre que les services soient prêts
Write-Host "⏳ Attente du démarrage des services..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Vérifier le statut des conteneurs
Write-Host "📊 Statut des services:" -ForegroundColor Cyan
docker-compose -f docker-compose.dev.yml ps

# Tester la santé de l'API
Write-Host "🩺 Test de santé de l'API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5041/health" -Method Get -TimeoutSec 5
    Write-Host "✅ API opérationnelle: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "⚠️  API pas encore prête, veuillez patienter..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎯 Environnement de développement prêt!" -ForegroundColor Green
Write-Host "📍 MongoDB: mongodb://localhost:27017" -ForegroundColor Cyan
Write-Host "📍 API: http://localhost:5041" -ForegroundColor Cyan
Write-Host "📍 Health Check: http://localhost:5041/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "🚀 Pour démarrer le frontend en mode développement:" -ForegroundColor Yellow
Write-Host "   cd frontend" -ForegroundColor White
Write-Host "   npm run dev" -ForegroundColor White
Write-Host ""
Write-Host "🛑 Pour arrêter les services:" -ForegroundColor Red
Write-Host "   docker-compose -f docker-compose.dev.yml down" -ForegroundColor White
