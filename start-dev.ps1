#!/usr/bin/env pwsh
# Script principal pour démarrer l'environnement de développement complet

Write-Host ""
Write-Host "🚀 QSO Manager - Environnement de Développement" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Vérifier les prérequis
Write-Host "🔍 Vérification des prérequis..." -ForegroundColor Cyan

# Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker n'est pas installé" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✅ Docker disponible" -ForegroundColor Green
}

# Docker Compose
if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker Compose n'est pas installé" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✅ Docker Compose disponible" -ForegroundColor Green
}

# Node.js
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Node.js n'est pas installé" -ForegroundColor Red
    exit 1
} else {
    $nodeVersion = node --version
    Write-Host "✅ Node.js disponible ($nodeVersion)" -ForegroundColor Green
}

# NPM
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Host "❌ NPM n'est pas installé" -ForegroundColor Red
    exit 1
} else {
    $npmVersion = npm --version
    Write-Host "✅ NPM disponible (v$npmVersion)" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎯 Choix du mode de démarrage :" -ForegroundColor Yellow
Write-Host "1. 🔧 Backend seulement (MongoDB + API)" -ForegroundColor White
Write-Host "2. 🌐 Frontend seulement (Vite dev server)" -ForegroundColor White
Write-Host "3. 🚀 Complet (Backend + Frontend)" -ForegroundColor White
Write-Host "4. 🛑 Arrêter l'environnement" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Votre choix (1-4)"

switch ($choice) {
    "1" {
        Write-Host "🔧 Démarrage du backend..." -ForegroundColor Cyan
        .\start-dev-backend.ps1
    }
    "2" {
        Write-Host "🌐 Démarrage du frontend..." -ForegroundColor Cyan
        cd frontend
        ..\start-dev-frontend.ps1
    }
    "3" {
        Write-Host "🚀 Démarrage complet..." -ForegroundColor Cyan
        
        # Démarrer le backend
        Write-Host "🔧 1/2 - Démarrage du backend..." -ForegroundColor Yellow
        .\start-dev-backend.ps1
        
        Write-Host ""
        Write-Host "⏳ Attente de 3 secondes avant le frontend..." -ForegroundColor Yellow
        Start-Sleep -Seconds 3
        
        # Démarrer le frontend dans un nouveau terminal
        Write-Host "🌐 2/2 - Ouverture du frontend..." -ForegroundColor Yellow
        Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PWD\frontend'; npm run dev"
        
        Write-Host ""
        Write-Host "🎉 Environnement complet démarré !" -ForegroundColor Green
        Write-Host "📱 Frontend: http://localhost:3000" -ForegroundColor Cyan
        Write-Host "📡 API: http://localhost:5041" -ForegroundColor Cyan
        Write-Host "🗄️  MongoDB: localhost:27017" -ForegroundColor Cyan
    }
    "4" {
        Write-Host "🛑 Arrêt de l'environnement..." -ForegroundColor Red
        .\stop-dev.ps1
    }
    default {
        Write-Host "❌ Choix invalide" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "✅ Opération terminée !" -ForegroundColor Green
