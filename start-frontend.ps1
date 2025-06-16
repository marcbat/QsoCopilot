# Script pour démarrer le frontend QSO Manager

# Ajouter Node.js au PATH
$env:PATH += ";C:\Program Files\nodejs"

# Afficher les versions
Write-Host "Node.js version:" -ForegroundColor Green
node --version
Write-Host "npm version:" -ForegroundColor Green
npm --version

# Aller dans le répertoire frontend
Set-Location -Path "c:\repos\QsoCopilot\frontend"

# Installer les dépendances si nécessaire
if (-not (Test-Path "node_modules")) {
    Write-Host "Installation des dépendances..." -ForegroundColor Yellow
    npm install
}

# Démarrer le serveur de développement
Write-Host "Démarrage du serveur de développement..." -ForegroundColor Green
npm run dev
