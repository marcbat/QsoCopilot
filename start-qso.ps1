# Script de gestion du projet QSO Manager
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "restart", "logs", "build", "clean")]
    [string]$Action
)

function Write-Header {
    param([string]$Title)
    Write-Host "`n=== $Title ===" -ForegroundColor Cyan
}

function Start-Services {
    Write-Header "Démarrage des services QSO Manager"
    
    # Vérifier si Docker est en cours d'exécution
    try {
        docker version | Out-Null
    }
    catch {
        Write-Error "Docker n'est pas en cours d'exécution. Veuillez démarrer Docker Desktop."
        return
    }

    Write-Host "Démarrage des conteneurs..." -ForegroundColor Green
    docker-compose up -d

    Write-Host "`nServices disponibles :" -ForegroundColor Yellow
    Write-Host "  - API Backend: http://localhost:5041" -ForegroundColor White
    Write-Host "  - API Documentation: http://localhost:5041/swagger" -ForegroundColor White
    Write-Host "  - Frontend React: http://localhost:3000" -ForegroundColor White
    Write-Host "  - MongoDB: localhost:27017" -ForegroundColor White
    
    Write-Host "`nPour voir les logs : .\start-qso.ps1 logs" -ForegroundColor Gray
}

function Stop-Services {
    Write-Header "Arrêt des services QSO Manager"
    docker-compose down
    Write-Host "Services arrêtés." -ForegroundColor Green
}

function Restart-Services {
    Write-Header "Redémarrage des services QSO Manager"
    docker-compose restart
    Write-Host "Services redémarrés." -ForegroundColor Green
}

function Show-Logs {
    Write-Header "Logs des services QSO Manager"
    Write-Host "Appuyez sur Ctrl+C pour quitter" -ForegroundColor Yellow
    docker-compose logs -f
}

function Build-Services {
    Write-Header "Construction des images Docker"
    docker-compose build --no-cache
    Write-Host "Images construites." -ForegroundColor Green
}

function Clean-Services {
    Write-Header "Nettoyage des ressources Docker"
    
    # Arrêter les services
    docker-compose down
    
    # Supprimer les images
    Write-Host "Suppression des images..." -ForegroundColor Yellow
    docker-compose down --rmi all --volumes --remove-orphans
    
    # Nettoyer les images non utilisées
    docker system prune -f
    
    Write-Host "Nettoyage terminé." -ForegroundColor Green
}

# Exécution de l'action demandée
switch ($Action) {
    "start" { Start-Services }
    "stop" { Stop-Services }
    "restart" { Restart-Services }
    "logs" { Show-Logs }
    "build" { Build-Services }
    "clean" { Clean-Services }
}

Write-Host "`nTerminé." -ForegroundColor Cyan
