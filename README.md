# QSO Manager

Application de log radioamateur avec backend .NET et frontend React.

## Architecture

- **Backend** : API REST en .NET 8 avec MongoDB
- **Frontend** : React 18 avec TypeScript
- **Base de données** : MongoDB
- **Authentification** : JWT

## Structure du projet

```
QsoCopliot/
├── backend/                      # API .NET
│   ├── QsoManager.Api/          # Couche présentation (API)
│   ├── QsoManager.Application/  # Couche application (Use Cases)
│   ├── QsoManager.Domain/       # Couche domaine (Event Sourcing)
│   └── QsoManager.Infrastructure/ # Couche infrastructure (MongoDB)
├── frontend/                    # Application React
└── docker-compose.yml          # Configuration Docker pour MongoDB
```

## Fonctionnalités

- [ ] **Architecture Event Sourcing**
  - [x] Agrégat QSOAggregate avec gestion des participants
  - [x] Events pour création, ajout/suppression de participants, réorganisation
  - [x] Result Pattern avec LanguageExt pour la gestion des erreurs
- [ ] Authentification utilisateur
- [ ] CRUD des QSOs via Event Sourcing
- [ ] Filtrage et recherche
- [ ] Interface utilisateur moderne
- [ ] Validation des données
- [ ] Gestion des erreurs avec Result Pattern

## Développement

### Backend
```bash
cd backend
dotnet run --project QsoManager.Api
```

### Frontend
```bash
cd frontend
npm start
```
