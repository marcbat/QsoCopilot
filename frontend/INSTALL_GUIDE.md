# Guide d'installation Node.js pour QSO Manager Frontend

## Étapes à suivre

### 1. Installer Node.js
- Télécharger Node.js depuis https://nodejs.org (version LTS recommandée)
- Installer avec les options par défaut
- Redémarrer VS Code après installation

### 2. Installer les dépendances
```powershell
cd c:\repos\QsoCopilot\frontend
npm install
```

### 3. Vérifier l'installation
```powershell
# Vérifier Node.js
node --version

# Vérifier npm
npm --version

# Lancer le projet en mode développement
npm run dev
```

## Problèmes résolus après installation

✅ **Types TypeScript** : Les définitions de types pour React, React Router, etc. seront disponibles
✅ **Support JSX** : Les éléments JSX seront correctement typés
✅ **Résolution des modules** : Import/export fonctionneront correctement
✅ **Compilation** : Le projet pourra être compilé et lancé
✅ **Hot reload** : Rechargement automatique pendant le développement

## Structure finale attendue après `npm install`

```
frontend/
├── node_modules/          # Dépendances installées
├── package-lock.json      # Verrous des versions
├── src/
│   ├── components/
│   │   ├── QsoManagerPage.tsx    ✅ Page principale
│   │   ├── QsoList.tsx           ✅ Liste des QSO
│   │   ├── QsoDetailPage.tsx     ✅ Détails QSO
│   │   ├── QsoEditPage.tsx       ✅ Édition QSO
│   │   ├── NewQsoForm.tsx        ✅ Formulaire création
│   │   ├── LoginPage.tsx         ✅ Connexion
│   │   ├── RegisterPage.tsx      ✅ Inscription
│   │   └── Header.tsx            ✅ Navigation
│   ├── contexts/
│   │   └── AuthContext.tsx       ✅ Authentification
│   ├── api/
│   │   └── index.ts              ✅ Services API
│   ├── types/
│   │   └── index.ts              ✅ Définitions TypeScript
│   └── styles/
│       └── global.css            ✅ Styles CSS
└── package.json
```

## Workflow complet une fois installé

1. **Page d'accueil** : Liste des QSO (accessible à tous)
2. **Visiteurs** : Boutons "S'inscrire" / "Se connecter" en haut à droite
3. **Utilisateurs connectés** : 
   - Formulaire de création QSO en haut
   - Boutons "Détails" et "Éditer" sur chaque QSO
   - Possibilité d'ajouter des participants

## Commandes de développement

```powershell
# Développement avec hot reload
npm run dev

# Construction pour production
npm run build

# Aperçu de la build de production
npm run preview

# Linting du code
npm run lint
```

## URLs de l'application

- **Frontend** : http://localhost:5173 (Vite dev server)
- **Backend API** : http://localhost:5041/api
- **Proxy configuré** : Les appels `/api/*` sont automatiquement redirigés vers le backend
