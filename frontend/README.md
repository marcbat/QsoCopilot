# Frontend QSO Manager

Interface React pour l'application QSO Manager.

## Prérequis

- Node.js 18+ (recommandé: 20.x LTS)
- npm ou yarn

## Installation

```powershell
# Installer Node.js depuis https://nodejs.org/

# Installer les dépendances
cd frontend
npm install
```

## Configuration

Le fichier `.env` contient la configuration de l'API :

```
VITE_API_URL=http://localhost:5041/api
```

## Démarrage

```powershell
# Démarrage en mode développement
npm run dev

# L'application sera disponible sur http://localhost:3000
```

## Architecture

### Workflow de Navigation

1. **Page principale** : Liste des QSO (accessible à tous)
   - Boutons "S'inscrire" et "Se connecter" en haut à droite pour les visiteurs
   - Formulaire de création de QSO horizontal en haut pour les utilisateurs connectés
   - Barre de recherche par nom
   - Liste des QSO avec détails

2. **Utilisateurs non connectés** :
   - Peuvent voir la liste des QSO
   - Peuvent rechercher les QSO
   - Accès aux pages de login/inscription

3. **Utilisateurs connectés** :
   - Toutes les fonctionnalités ci-dessus
   - Formulaire de création de QSO en haut de la page principale
   - Possibilité d'ajouter des participants aux QSO créés
   - Bouton de déconnexion dans le header

### Structure des Composants

```
src/
├── components/
│   ├── Header.tsx           # Navigation et authentification
│   ├── QsoManagerPage.tsx   # Page principale avec liste des QSO
│   ├── QsoList.tsx          # Affichage de la liste des QSO
│   ├── NewQsoForm.tsx       # Formulaire de création QSO (horizontal)
│   ├── LoginPage.tsx        # Page de connexion
│   └── RegisterPage.tsx     # Page d'inscription
├── contexts/
│   └── AuthContext.tsx      # Gestion de l'authentification
├── api/
│   └── index.ts            # Services API
├── types/
│   └── index.ts            # Types TypeScript
└── styles/
    └── global.css          # Styles CSS globaux
```

### Fonctionnalités

- **Authentification** : Login par nom d'utilisateur ou email
- **Gestion des QSO** : Création, consultation, recherche
- **Gestion des participants** : Ajout de participants aux QSO
- **Interface responsive** : Adaptée mobile et desktop
- **Design moderne** : Interface claire et intuitive

## API

L'application communique avec l'API backend (.NET) via les endpoints :

- `GET /api/QsoAggregate` - Liste des QSO
- `POST /api/QsoAggregate` - Créer un QSO (authentifié)
- `POST /api/QsoAggregate/{id}/participants` - Ajouter un participant (authentifié)
- `GET /api/QsoAggregate/search?name=...` - Rechercher par nom
- `POST /api/Auth/login` - Connexion
- `POST /api/Auth/register` - Inscription

## Scripts

- `npm run dev` - Démarrage en développement
- `npm run build` - Construction pour production
- `npm run preview` - Prévisualiser la version de production
- `npm run lint` - Vérification du code
