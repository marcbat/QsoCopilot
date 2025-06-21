#!/usr/bin/env pwsh
# Script de test pour la fonctionnalité de suppression QSO

Write-Host "🧪 Test de la fonctionnalité de suppression QSO" -ForegroundColor Green

# Vérifier que les services sont démarrés
Write-Host "`n📋 Vérification des services..." -ForegroundColor Yellow

# Test de l'API backend
try {
    $healthCheck = Invoke-RestMethod -Uri "http://localhost:5041/Health" -Method Get
    Write-Host "✅ Backend API : $($healthCheck.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend API : Non disponible" -ForegroundColor Red
    Write-Host "   Assurez-vous que docker-compose est démarré" -ForegroundColor Yellow
}

# Test du frontend
try {
    $frontendCheck = Invoke-WebRequest -Uri "http://localhost:3001" -Method Head -ErrorAction Stop
    Write-Host "✅ Frontend : Disponible (Status: $($frontendCheck.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ Frontend : Non disponible" -ForegroundColor Red
    Write-Host "   Exécutez: npm run dev dans le dossier frontend" -ForegroundColor Yellow
}

Write-Host "`n🎯 Pour tester la suppression:" -ForegroundColor Cyan
Write-Host "1. Ouvrir http://localhost:3001" -ForegroundColor White
Write-Host "2. Se connecter avec un compte utilisateur" -ForegroundColor White
Write-Host "3. Dans la liste des QSO, chercher le bouton rouge 'Supprimer'" -ForegroundColor White
Write-Host "4. Cliquer sur 'Supprimer' et confirmer" -ForegroundColor White
Write-Host "5. Vérifier que le QSO disparaît de la liste" -ForegroundColor White

Write-Host "`n⚠️  Rappel important:" -ForegroundColor Yellow
Write-Host "- Seuls les modérateurs peuvent supprimer les QSO" -ForegroundColor White
Write-Host "- Une confirmation est obligatoire avant suppression" -ForegroundColor White
Write-Host "- La suppression est irréversible" -ForegroundColor White

Write-Host "`n🔧 Tests d'intégration disponibles:" -ForegroundColor Cyan
Write-Host "cd test/QsoManager.IntegrationTests" -ForegroundColor Gray
Write-Host "dotnet test --filter QsoAggregateControllerDeleteTests" -ForegroundColor Gray
