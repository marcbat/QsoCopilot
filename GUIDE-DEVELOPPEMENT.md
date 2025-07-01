# 🚀 Guide de Développement - QSO Manager

Ce guide explique comment utiliser l'environnement de développement optimisé pour un développement frontend rapide.

## 📋 Architecture de Développement

L'environnement de développement est séparé en deux parties :

1. **Backend (Docker)** : MongoDB + API .NET
2. **Frontend (Local)** : Serveur de développement Vite avec hot-reload

Cette séparation permet un développement frontend ultra-rapide avec rechargement instantané des modifications.

## 🛠️ Prérequis

- Docker Desktop
- Node.js (v18+)
- PowerShell Core

## 🚀 Démarrage Rapide

### Option 1 : Script Interactif (Recommandé)
```powershell
.\start-dev.ps1
```

### Option 2 : Démarrage Manuel

#### 1. Backend (MongoDB + API)
```powershell
.\start-dev-backend.ps1
```

#### 2. Frontend (Vite Dev Server)
```powershell
cd frontend
..\start-dev-frontend.ps1
```

## 🌐 URLs de Développement

- **Frontend** : http://localhost:3000 (Hot-reload activé)
- **API** : http://localhost:5041
- **MongoDB** : localhost:27017

## 📁 Structure des Scripts

| Script | Description |
|--------|-------------|
| `start-dev.ps1` | Script interactif principal |
| `start-dev-backend.ps1` | Démarre MongoDB + API en Docker |
| `start-dev-frontend.ps1` | Démarre le frontend Vite |
| `stop-dev.ps1` | Arrête l'environnement de développement |

## 🔧 Configuration

### Docker Compose Dev
Le fichier `docker-compose.dev.yml` contient uniquement :
- MongoDB (port 27017)
- API .NET (port 5041)

### Vite Configuration
Le proxy Vite redirige automatiquement `/api/*` vers `http://localhost:5041`

## 💡 Avantages du Mode Développement

### ⚡ Développement Frontend Ultra-Rapide
- **Hot Module Replacement (HMR)** : Modifications instantanées
- **Rechargement rapide** : ~100ms vs plusieurs secondes avec Docker
- **Debugging facilité** : Source maps natifs

### 🔄 Workflow Optimisé
1. Démarrer le backend une fois
2. Développer le frontend avec rechargement instantané
3. Tester les API directement

### 🐳 Isolation Backend
- MongoDB et API isolés dans Docker
- Pas d'impact sur l'environnement local
- Données persistantes entre redémarrages

## 🧪 Tests et Debugging

### Test de l'API
```powershell
# Health check
curl http://localhost:5041/health

# Liste des QSO
curl http://localhost:5041/api/QsoAggregate
```

### Logs des Conteneurs
```powershell
# Logs de l'API
docker logs qso-manager-api-dev -f

# Logs MongoDB
docker logs qso-manager-mongodb-dev -f
```

### Statut des Services
```powershell
docker-compose -f docker-compose.dev.yml ps
```

## 🔧 Dépannage

### Port déjà utilisé
```powershell
# Vérifier les ports utilisés
netstat -ano | findstr :3000
netstat -ano | findstr :5041
```

### Reset complet
```powershell
# Arrêter tout
.\stop-dev.ps1
docker system prune -f

# Redémarrer
.\start-dev.ps1
```

### Problème de proxy Vite
Vérifier la configuration dans `frontend/vite.config.ts` :
```typescript
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5041',
      changeOrigin: true,
      secure: false,
    }
  }
}
```

## 📦 Production

Pour le déploiement en production, utiliser le Docker Compose complet :
```powershell
docker-compose up -d
```

## 🎯 Workflow Recommandé

1. **Démarrage** : `.\start-dev.ps1` → Choix 3 (Complet)
2. **Développement** : Modifier les fichiers dans `frontend/src/`
3. **Test** : Vérification automatique via hot-reload
4. **API** : Tester avec `curl` ou Postman
5. **Arrêt** : `.\stop-dev.ps1`

## 🔄 Migration depuis l'Ancien Workflow

### Avant (Docker complet)
```powershell
docker-compose up -d  # ~30s de build + lent à redémarrer
```

### Maintenant (Dev séparé)
```powershell
.\start-dev.ps1      # ~10s backend + hot-reload instantané
```

**Gain de productivité** : 10x plus rapide pour le développement frontend ! 🚀
