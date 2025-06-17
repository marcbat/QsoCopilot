# QsoManager.Application.UnitTests

Ce projet contient les tests unitaires pour la couche Application du projet QsoManager. Il utilise xUnit, NSubstitute et Verify pour les tests et vérifications.

## Technologies utilisées

- **xUnit** - Framework de tests
- **NSubstitute** - Framework de mocking
- **Verify** - Framework de vérification de snapshots

## Structure des tests

### Commands.QsoAggregate
- [`AddParticipantCommandHandlerTests`](Commands/QsoAggregate/AddParticipantCommandHandlerTests.cs) - Tests pour le handler d'ajout de participant

## Tests couverts pour AddParticipantCommandHandler

### Scénarios de succès
- `Handle_WhenValidRequest_ShouldAddParticipantSuccessfully` - Test du cas nominal d'ajout d'un participant

### Scénarios d'erreur d'authentification
- `Handle_WhenUserIdClaimMissing_ShouldReturnAuthenticationError` - Test quand l'utilisateur n'a pas de claim d'identité
- `Handle_WhenUserIdClaimInvalid_ShouldReturnAuthenticationError` - Test quand le claim d'identité n'est pas un GUID valide

### Scénarios d'erreur d'autorisation
- `Handle_WhenUserNotModerator_ShouldReturnAuthorizationError` - Test quand l'utilisateur n'est pas le modérateur du QSO

### Scénarios d'erreur de repository
- `Handle_WhenAggregateNotFound_ShouldReturnRepositoryError` - Test quand l'agrégat n'existe pas
- `Handle_WhenSaveAggregateFails_ShouldReturnRepositoryError` - Test quand la sauvegarde échoue

### Scénarios d'erreur de domaine
- `Handle_WhenAddParticipantFails_ShouldReturnDomainError` - Test quand l'ajout de participant échoue (indicatif vide)

### Scénarios d'erreur exceptionnelle
- `Handle_WhenExceptionThrown_ShouldReturnGenericError` - Test de gestion des exceptions non prévues

## Exécution des tests

Pour exécuter tous les tests unitaires :

```bash
dotnet test test/QsoManager.Application.UnitTests/QsoManager.Application.UnitTests.csproj
```

Pour exécuter avec plus de détails :

```bash
dotnet test test/QsoManager.Application.UnitTests/QsoManager.Application.UnitTests.csproj --verbosity normal
```

## Couverture des tests

Les tests couvrent :
- ✅ Validation des claims utilisateur
- ✅ Autorisation (modérateur uniquement)
- ✅ Gestion des erreurs de repository
- ✅ Gestion des erreurs de domaine
- ✅ Gestion des exceptions
- ✅ Vérification des interactions avec les mocks
- ✅ Validation des DTOs retournés

## Conventions de test

- Les tests suivent le pattern **Arrange-Act-Assert**
- Les noms de méthodes suivent la convention `Handle_When{Scenario}_Should{ExpectedResult}`
- Utilisation de mocks pour toutes les dépendances externes
- Vérification des interactions avec les mocks via `NSubstitute.Received()`
