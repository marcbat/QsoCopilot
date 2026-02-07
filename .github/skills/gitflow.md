# Workflow Git (GitFlow)

## Stratégie de Branches

Ce projet utilise **GitFlow** comme stratégie de gestion des branches :

- **`main`** : Branche principale de production, contient uniquement du code stable et testé
- **`develop`** : Branche de développement, intégration continue des features
- **`feature/theme-de-la-feature`** : Branches de fonctionnalités créées depuis `develop`
- **`hotfix/description-du-fix`** : Correctifs urgents créés depuis `main`
- **`release/version`** : Préparation des versions avant fusion dans `main`

## Workflow de Développement

### Créer une Nouvelle Feature
```bash
# Depuis develop
git checkout develop
git pull origin develop
git checkout -b feature/nom-de-la-fonctionnalite

# Développement et commits...
git add .
git commit -m "feat: description de la fonctionnalité"

# Push de la feature
git push origin feature/nom-de-la-fonctionnalite

# Créer une Pull Request vers develop
```

### Finaliser une Feature
```bash
# Merge dans develop (via Pull Request recommandé)
git checkout develop
git merge feature/nom-de-la-fonctionnalite
git push origin develop

# Supprimer la branche feature
git branch -d feature/nom-de-la-fonctionnalite
git push origin --delete feature/nom-de-la-fonctionnalite
```

## Messages de Commit

**OBLIGATION : Tous les messages de commit DOIVENT être en français.**

### Format des Commits (Convention Conventional Commits)

```
<type>: <description courte en français>

[corps optionnel avec plus de détails]

[footer optionnel - références issues, breaking changes]
```

### Types de Commits Autorisés

- **feat** : Nouvelle fonctionnalité
  ```
  feat: ajout de la gestion des participants QSO
  ```

- **fix** : Correction de bug
  ```
  fix: correction de l'ordre des participants après réorganisation
  ```

- **refactor** : Refactoring sans changement de fonctionnalité
  ```
  refactor: extraction de la logique de validation dans un service dédié
  ```

- **docs** : Documentation uniquement
  ```
  docs: mise à jour du README avec les instructions Docker
  ```

- **style** : Formatage, points-virgules manquants (pas de changement de code)
  ```
  style: application des règles ESLint sur les composants React
  ```

- **test** : Ajout ou modification de tests
  ```
  test: ajout des tests unitaires pour QsoAggregate
  ```

- **chore** : Tâches de maintenance (dépendances, configuration)
  ```
  chore: mise à jour des packages npm vers les dernières versions
  ```

- **perf** : Amélioration des performances
  ```
  perf: optimisation des requêtes MongoDB avec index
  ```

- **build** : Modifications du système de build
  ```
  build: configuration du Dockerfile pour multi-stage build
  ```

- **ci** : Modifications des fichiers CI/CD
  ```
  ci: ajout du workflow GitHub Actions pour les tests
  ```

### Exemples de Bons Messages

✅ **Commit simple**
```
feat: ajout du support SignalR pour les mises à jour temps réel
```

✅ **Commit avec corps**
```
feat: implémentation du pattern Event Sourcing

- Ajout de l'EventRepository avec MongoDB
- Création de l'AggregateRoot abstrait
- Implémentation du versioning des événements
```

✅ **Commit avec référence issue**
```
fix: correction du bug de duplication des participants

Résout le problème où un participant pouvait être ajouté deux fois
lors d'un clic rapide sur le bouton.

Closes #42
```

✅ **Breaking change**
```
feat!: migration vers .NET 9

BREAKING CHANGE: Nécessite .NET 9 SDK pour compiler le projet
```

### Exemples à Éviter

❌ **Messages en anglais** (INTERDIT)
```
feat: add participant management
```

❌ **Messages trop vagues**
```
fix: correction bug
```

❌ **Messages sans type**
```
ajout de la fonctionnalité
```

## Commandes Git Utiles

```bash
# Vérifier sur quelle branche on est
git branch

# Voir l'état des modifications
git status

# Voir l'historique des commits
git log --oneline --graph --decorate

# Synchroniser avec develop
git checkout develop
git pull origin develop

# Créer une feature depuis develop à jour
git checkout -b feature/nouvelle-fonctionnalite

# Rebaser une feature sur develop
git checkout feature/ma-feature
git rebase develop

# Voir les différences avant commit
git diff

# Commit interactif (choisir les fichiers)
git add -p
```
