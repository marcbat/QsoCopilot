## Script de configuration des certificats pour QSO Copilot
Write-Host "Configuration des certificats HTTPS pour QSO Copilot" -ForegroundColor Green

# Nettoyer les certificats existants
Write-Host "Nettoyage des certificats de développement existants..." -ForegroundColor Yellow
dotnet dev-certs https --clean

# Créer et approuver un nouveau certificat
Write-Host "Création et approbation d'un nouveau certificat..." -ForegroundColor Yellow
dotnet dev-certs https --trust

# Exporter le certificat pour Docker
$certPassword = "qsocopilot"
$certPath = "$env:APPDATA\ASP.NET\Https"
$certFile = "QsoCopilot.pfx"

Write-Host "Exportation du certificat pour Docker..." -ForegroundColor Yellow
New-Item -Force -Path $certPath -Type Directory
dotnet dev-certs https -ep "$certPath\$certFile" -p $certPassword

Write-Host "Configuration des certificats terminée avec succès!" -ForegroundColor Green
Write-Host "Vous pouvez maintenant exécuter le projet Docker Compose dans Visual Studio 2022" -ForegroundColor Green
