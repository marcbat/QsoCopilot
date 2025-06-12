# QSO Manager

Application complète de gestion de QSO (contacts radioamateurs) avec architecture Event Sourcing, backend .NET 9 et MongoDB.

## Architecture

- **Backend** : API REST en .NET 9 avec architecture Event Sourcing
- **Base de données** : MongoDB (Event Store + Projections)
- **Frontend** : React 18 avec TypeScript (en développement)
- **Conteneurisation** : Docker Compose

### Patterns et Technologies

- **Event Sourcing** : Persistance des événements avec reconstruction d'état
- **CQRS** : Séparation commandes/lectures avec projections MongoDB
- **Result Pattern** : Gestion d'erreurs fonctionnelle avec LanguageExt
- **MediatR** : Bus de messages pour CQRS
- **Background Services** : Traitement asynchrone des projections

## Structure du projet

```
QsoCopilot/
├── backend/                           # API .NET 9
│   ├── QsoManager.Api/               # Couche présentation (API REST)
│   │   ├── Controllers/              # Contrôleurs API
│   │   │   ├── QsoAggregateController.cs    # CRUD QSO avec participants
│   │   │   ├── QsoProjectionsController.cs # Lecture des projections
│   │   │   └── ReprojectionController.cs    # Administration Event Store
│   │   └── Program.cs
│   ├── QsoManager.Application/       # Couche application (Use Cases)
│   │   ├── Commands/                 # Commands CQRS
│   │   │   └── QsoAggregate/        # Commandes QSO
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Projections/             # Services de projection
│   │   │   ├── Services/            # Gestionnaire projections
│   │   │   └── Interfaces/          # Contrats projections
│   │   └── Interfaces/              # Contrats généraux
│   ├── QsoManager.Domain/           # Couche domaine (Event Sourcing)
│   │   ├── Aggregates/             # Agrégats DDD
│   │   │   ├── QsoAggregate.cs     # Agrégat principal QSO
│   │   │   └── ModeratorAggregate.cs # Agrégat modérateur
│   │   ├── Common/                  # Base classes Event Sourcing
│   │   ├── Entities/               # Entités domaine
│   │   └── Events/                 # Événements domaine
│   └── QsoManager.Infrastructure/   # Couche infrastructure
│       ├── Repositories/           # Implémentations persistance
│       │   ├── EventRepository.cs  # Event Store MongoDB
│       │   └── QsoAggregateRepository.cs
│       └── Projections/            # Read models MongoDB
├── frontend/                        # Application React (TODO)
├── test/                           # Tests d'intégration
│   └── QsoManager.IntegrationTests/
├── mongo-init/                     # Scripts initialisation MongoDB
└── docker-compose.yml             # Orchestration conteneurs
```

## Fonctionnalités Implémentées

### ✅ Architecture Event Sourcing
- **Agrégats DDD** : QsoAggregate et ModeratorAggregate
- **Event Store** : Persistance MongoDB avec versioning
- **Projections CQRS** : Read models optimisés pour les lectures
- **Result Pattern** : Gestion d'erreurs avec LanguageExt
- **Background Processing** : Traitement asynchrone des événements

### ✅ Gestion des QSO
- **Création de QSO** : Avec nom, description et modérateur
- **Validation unicité** : Noms de QSO uniques dans le système
- **Gestion des participants** :
  - Ajout de participants avec indicatifs
  - Suppression de participants
  - Réorganisation des ordres
  - Déplacement vers position spécifique

### ✅ API REST complète
- **QsoAggregateController** :
  - `POST /api/QsoAggregate` - Créer un QSO
  - `POST /api/QsoAggregate/{id}/participants` - Ajouter participant
  - `DELETE /api/QsoAggregate/{id}/participants/{callSign}` - Supprimer participant
  - `PUT /api/QsoAggregate/{id}/participants/reorder` - Réorganiser participants
  - `PUT /api/QsoAggregate/{id}/participants/{callSign}/move` - Déplacer participant

- **QsoProjectionsController** :
  - `GET /api/QsoProjections` - Lister tous les QSO
  - `GET /api/QsoProjections/{id}` - QSO par ID
  - `GET /api/QsoProjections/search?name=...` - Recherche par nom

- **ReprojectionController** :
  - `POST /api/Reprojection/start` - Redémarrer projections
  - `GET /api/Reprojection/status/{taskId}` - Statut tâche
  - `GET /api/Reprojection/status` - Toutes les tâches

### ✅ Système de Projections
- **Projections temps réel** : Mise à jour automatique via événements
- **Reprojection complète** : Reconstruction des read models
- **Monitoring des tâches** : Suivi du progrès et erreurs
- **Recherche optimisée** : Index MongoDB pour performances

### ✅ Tests d'intégration
- Tests avec base de données MongoDB réelle
- Validation des scénarios complets
- Snapshots de régression (Verify)
- Couverture des cas d'erreur

### ✅ Infrastructure
- **Docker Compose** : MongoDB + API
- **Health Checks** : Surveillance des services
- **Logging structuré** : Microsoft.Extensions.Logging
- **Configuration** : appsettings par environnement

## En cours de développement

### 🚧 Interface utilisateur
- Interface React moderne
- Gestion des QSO en temps réel
- Dashboard de monitoring

### 🚧 Fonctionnalités avancées
- Authentification JWT
- Autorisation par rôles
- Export des données
- Statistiques et rapports

## Installation et utilisation

### Prérequis
- Docker et Docker Compose
- .NET 9 SDK (pour développement)
- MongoDB (via Docker ou local)

### Démarrage avec Docker
```bash
# Démarrer tous les services
docker compose up -d

# Vérifier les logs
docker compose logs -f

# API disponible sur http://localhost:5041
# MongoDB sur localhost:27017
```

### Développement local

#### Backend
```bash
cd backend
dotnet restore
dotnet run --project QsoManager.Api

# API disponible sur http://localhost:5041
```

#### Tests
```bash
cd backend
dotnet test

# Tests d'intégration avec MongoDB
cd test/QsoManager.IntegrationTests
dotnet test
```

### Endpoints API

#### Santé de l'API
```bash
GET http://localhost:5041/health
```

#### Créer un QSO
```bash
POST http://localhost:5041/api/QsoAggregate
Content-Type: application/json

{
  "name": "QSO Test",
  "description": "Mon premier QSO",
  "moderatorId": "123e4567-e89b-12d3-a456-426614174000"
}
```

#### Ajouter un participant
```bash
POST http://localhost:5041/api/QsoAggregate/{qsoId}/participants
Content-Type: application/json

{
  "callSign": "F1ABC"
}
```

#### Lister les QSO
```bash
GET http://localhost:5041/api/QsoProjections
```

## Configuration

### Variables d'environnement
```bash
# MongoDB
MONGO_DATABASE=QsoManager
MONGO_CONNECTION_STRING=mongodb://localhost:27017

# API
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
```

### Structure MongoDB
- **Base Events** : `QsoManager.Events`
- **Base Projections** : `QsoManager.QsoAggregateProjections`
- **Collections** : Séparation event store / read models

## Monitoring et Administration

### Reprojection complète
```bash
# Démarrer une reprojection
POST http://localhost:5041/api/Reprojection/start

# Suivre le progrès
GET http://localhost:5041/api/Reprojection/status/{taskId}
```

### Logs
```bash
# Logs temps réel
docker compose logs -f qso-api

# Logs MongoDB
docker compose logs -f mongodb
```

## Contributing

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/amazing-feature`)
3. Commit les changements (`git commit -m 'Add amazing feature'`)
4. Push vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrir une Pull Request

## License

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.
