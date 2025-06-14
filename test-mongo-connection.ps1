# Script pour tester la connexion MongoDB depuis l'extérieur de Docker
Write-Host "=== Test de connexion MongoDB externe ===" -ForegroundColor Green

# Test 1: Vérification que le port est ouvert
Write-Host "`n1. Test de connectivité TCP sur le port 27017..." -ForegroundColor Yellow
$tcpTest = Test-NetConnection -ComputerName localhost -Port 27017
if ($tcpTest.TcpTestSucceeded) {
    Write-Host "✅ Port 27017 accessible" -ForegroundColor Green
} else {
    Write-Host "❌ Port 27017 non accessible" -ForegroundColor Red
    exit 1
}

# Test 2: Test de connexion MongoDB via Docker exec (validation interne)
Write-Host "`n2. Test de connexion MongoDB interne..." -ForegroundColor Yellow
$mongoTest = docker exec -it qso-manager-mongodb mongosh --eval "db.adminCommand('ping')" --authenticationDatabase admin -u admin -p password123 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ MongoDB fonctionne correctement à l'intérieur du conteneur" -ForegroundColor Green
} else {
    Write-Host "❌ Problème avec MongoDB dans le conteneur" -ForegroundColor Red
    exit 1
}

# Test 3: Affichage des informations de connexion
Write-Host "`n3. Informations de connexion externes:" -ForegroundColor Yellow
Write-Host "   Host: localhost" -ForegroundColor Cyan
Write-Host "   Port: 27017" -ForegroundColor Cyan
Write-Host "   Username: admin" -ForegroundColor Cyan
Write-Host "   Password: password123" -ForegroundColor Cyan
Write-Host "   Database: QsoManager" -ForegroundColor Cyan
Write-Host "   Connection String: mongodb://admin:password123@localhost:27017/QsoManager?authSource=admin" -ForegroundColor Cyan

Write-Host "`n4. Commandes pour tester avec des outils externes:" -ForegroundColor Yellow
Write-Host "   MongoDB Compass: mongodb://admin:password123@localhost:27017/QsoManager?authSource=admin" -ForegroundColor Magenta
Write-Host "   mongosh (si installé): mongosh mongodb://admin:password123@localhost:27017/QsoManager --authenticationDatabase admin" -ForegroundColor Magenta

Write-Host "`n=== MongoDB est maintenant accessible depuis l'extérieur de Docker ===" -ForegroundColor Green
