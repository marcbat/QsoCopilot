# Script PowerShell pour gérer Docker Compose
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("up", "down", "build", "logs", "restart")]
    [string]$Action
)

$dockerComposePath = "docker-compose.yml"

switch ($Action) {
    "up" {
        Write-Host "🚀 Démarrage des services QSO Manager..." -ForegroundColor Green
        docker-compose -f $dockerComposePath up -d
        
        Write-Host "⏳ Attente du démarrage des services..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        Write-Host "📊 État des conteneurs:" -ForegroundColor Cyan
        docker-compose -f $dockerComposePath ps
        
        Write-Host "🌐 URLs disponibles:" -ForegroundColor Green
        Write-Host "  - API: http://localhost:5041" -ForegroundColor White
        Write-Host "  - Swagger: http://localhost:5041/swagger" -ForegroundColor White
        Write-Host "  - MongoDB: mongodb://localhost:27017" -ForegroundColor White
    }
    
    "down" {
        Write-Host "🛑 Arrêt des services QSO Manager..." -ForegroundColor Red
        docker-compose -f $dockerComposePath down
    }
    
    "build" {
        Write-Host "🔨 Reconstruction des images..." -ForegroundColor Blue
        docker-compose -f $dockerComposePath build --no-cache
    }
    
    "logs" {
        Write-Host "📋 Affichage des logs..." -ForegroundColor Cyan
        docker-compose -f $dockerComposePath logs -f
    }
    
    "restart" {
        Write-Host "🔄 Redémarrage des services..." -ForegroundColor Yellow
        docker-compose -f $dockerComposePath restart
    }
}

Write-Host "✅ Action '$Action' terminée!" -ForegroundColor Green
