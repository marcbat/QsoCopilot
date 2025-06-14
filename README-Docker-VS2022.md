# QSO Copilot - Configuration Docker Compose pour Visual Studio 2022

Ce projet contient la configuration Docker Compose pour développer et déboguer l'application QSO Copilot avec Visual Studio 2022.

## Prérequis

- Visual Studio 2022 avec les charges de travail :
  - Développement ASP.NET et web
  - Outils de conteneur
- Docker Desktop pour Windows
- .NET 9.0 SDK

## Structure du projet

```
QsoCopilot/
├── QsoCopilot.sln              # Solution principale avec Docker Compose
├── docker-compose.dcproj        # Projet Docker Compose pour VS2022
├── docker-compose.yml           # Configuration Docker Compose
├── docker-compose.override.yml  # Configuration de développement
├── launchSettings.json          # Profils de lancement
├── backend/                     # Code source de l'API
│   ├── QsoManager.sln          # Solution backend uniquement
│   ├── Dockerfile              # Image Docker pour l'API
│   └── ...
├── frontend/                    # Code source du frontend (React)
└── mongo-init/                  # Scripts d'initialisation MongoDB
```

## Utilisation avec Visual Studio 2022

### 1. Configuration des certificats HTTPS

**Important** : Avant de lancer le projet, vous devez configurer les certificats HTTPS pour le développement :

1. Ouvrez PowerShell en tant qu'administrateur
2. Naviguez vers le dossier racine du projet
3. Exécutez le script `setup-certificates.ps1` :
   ```powershell
   .\setup-certificates.ps1
   ```
4. Confirmez toutes les demandes de confiance de certificat

Ce script configure les certificats nécessaires pour le développement HTTPS dans Docker.

### 2. Ouverture du projet

1. Ouvrez Visual Studio 2022
2. Ouvrez le fichier `QsoCopilot.sln` à la racine du projet
3. Vous devriez voir le projet `docker-compose` dans l'Explorateur de solutions

### 3. Configuration de débogage

1. Sélectionnez `Docker Compose` comme projet de démarrage
2. Choisissez le profil `Docker Compose` dans la barre d'outils
3. Cliquez sur F5 ou le bouton "Démarrer le débogage"

Si vous rencontrez des problèmes de certificat :
- Vérifiez que le script `setup-certificates.ps1` a été exécuté avec succès
- Assurez-vous que le certificat `QsoCopilot.pfx` existe dans `%APPDATA%\ASP.NET\Https`
- Le mot de passe du certificat est configuré dans `docker-compose.override.yml`

### 4. Services disponibles

Une fois lancé, les services suivants seront disponibles :

- **API QSO Manager** : `https://localhost:5001` ou `http://localhost:5000`
- **Swagger UI** : `https://localhost:5001/swagger`
- **MongoDB** : `localhost:27017`
  - Utilisateur : `admin`
  - Mot de passe : `password123`
  - Base de données : `QsoManager`

### 4. Débogage

Visual Studio 2022 permet de :
- Mettre des points d'arrêt dans le code C#
- Déboguer directement dans les conteneurs
- Voir les logs des conteneurs dans la fenêtre "Sortie"
- Redémarrer les services individuellement

### 5. Variables d'environnement

Les variables d'environnement sont configurées dans :
- `docker-compose.yml` : Configuration de production
- `docker-compose.override.yml` : Configuration de développement

Pour le développement, les certificats HTTPS et les secrets utilisateur sont automatiquement montés.

## Commandes utiles

### Via Visual Studio 2022
- **F5** : Démarrer avec débogage
- **Ctrl+F5** : Démarrer sans débogage
- **Maj+F5** : Arrêter le débogage
- **Ctrl+Maj+F5** : Redémarrer

### Via ligne de commande
```powershell
# Démarrer tous les services
docker-compose up -d

# Voir les logs
docker-compose logs -f

# Arrêter tous les services
docker-compose down

# Reconstruire et démarrer
docker-compose up --build -d
```

## Résolution de problèmes

### Docker Desktop n'est pas démarré
Assurez-vous que Docker Desktop est en cours d'exécution avant de lancer le projet.

### Ports déjà utilisés
Si les ports 5000/5001 sont utilisés, modifiez les dans `docker-compose.override.yml`.

### Problèmes de certificats HTTPS
Les certificats de développement sont automatiquement montés. Si vous rencontrez des problèmes :
```powershell
dotnet dev-certs https --trust
```

### Base de données non initialisée
Si MongoDB ne démarre pas correctement, supprimez le volume et redémarrez :
```powershell
docker-compose down -v
docker-compose up -d
```

## Configuration avancée

### Modification des ports
Éditez `docker-compose.override.yml` pour changer les ports :
```yaml
services:
  qso-api:
    ports:
      - "8000:80"
      - "8001:443"
```

### Variables d'environnement personnalisées
Créez un fichier `.env` à la racine pour définir des variables personnalisées :
```
MONGODB_PASSWORD=motdepasse_personnalise
API_PORT=8080
```

### Montage de volumes supplémentaires
Ajoutez des volumes dans `docker-compose.override.yml` :
```yaml
services:
  qso-api:
    volumes:
      - ./data:/app/data
```
