# Script pour arrêter l'environnement de développement

Write-Host "🛑 Arrêt de l'environnement de développement..." -ForegroundColor Red

# Arrêter les conteneurs de développement
Write-Host "📦 Arrêt des conteneurs backend..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml down

Write-Host "✅ Environnement de développement arrêté" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Pour redémarrer:" -ForegroundColor Cyan
Write-Host "   ./start-dev-backend.ps1" -ForegroundColor White
Write-Host "   cd frontend && ../start-dev-frontend.ps1" -ForegroundColor White
