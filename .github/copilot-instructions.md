# QSO Manager - Instructions pour les Agents IA

## Objectif du Projet

Cette application gère des **QSO** (contacts radioamateurs) avec un support complet du cycle de vie :
- Créer/lister des QSO avec des participants (opérateurs radio identifiés par leurs indicatifs)
- Gérer l'ordre des participants et leurs mouvements (QRV/QRT - arrivée/départ)
- Visualiser les participants sous forme de liste triable ou de carte géographique (via les locators QTH)
- Mises à jour en temps réel via SignalR pour une gestion collaborative des QSO

## Architecture

**Event Sourcing + CQRS** avec backend .NET 9 et frontend React 18 :
- **Event Store** : MongoDB stocke tous les événements de domaine ([EventRepository.cs](backend/QsoManager.Infrastructure/Repositories/EventRepository.cs))
- **Agrégats** : [QsoAggregate.cs](backend/QsoManager.Domain/Aggregates/QsoAggregate.cs) avec classe statique `Events` imbriquée
- **Commandes** : Opérations d'écriture via MediatR ([Commands/QsoAggregate/](backend/QsoManager.Application/Commands/QsoAggregate/))
- **Projections** : Service en arrière-plan met à jour les modèles de lecture depuis le flux d'événements ([ProjectionDispatcherService.cs](backend/QsoManager.Application/Projections/Services/ProjectionDispatcherService.cs))
- **Requêtes** : Lecture depuis les projections MongoDB, pas depuis l'event store ([Queries/QsoAggregate/](backend/QsoManager.Application/Queries/QsoAggregate/))

## Style de Code

### Backend C#
- **Result Pattern** : Retourner `Validation<Error, T>` depuis LanguageExt, utiliser `Match` pour le traitement
- **Immutabilité** : Records pour DTOs/Commandes/Événements, collections IReadOnly
- **Nommage** : `_camelCase` pour champs privés, `PascalCase` pour méthodes, événements imbriqués comme `QsoAggregate.Events.Created`
- **Validation** : Composer avec syntaxe de requête LINQ et `.Apply()` pour validation applicative
- **Logging** : Logs structurés avec paramètres nommés : `_logger.LogInformation("Message {Param}", value)`

### Frontend TypeScript/React
- **Composants Fonctionnels** : Utiliser les hooks (`useState`, `useEffect`, hooks personnalisés comme `useQsoSignalR`)
- **Couche API** : [qsoApi.ts](frontend/src/api/qsoApi.ts) centralisé avec intercepteurs axios pour JWT
- **Contexte** : [AuthContext.tsx](frontend/src/contexts/AuthContext.tsx) pour l'état global
- **Temps réel** : SignalR via hooks personnalisés, rejoindre les groupes avec pattern `qso_{qsoId}`

## Build et Test

```powershell
# Backend - Exécuter depuis la racine du dépôt
dotnet build backend/QsoManager.sln
dotnet run --project backend/QsoManager.Api
dotnet test

# Frontend - Exécuter depuis le répertoire frontend
cd frontend
npm install
npm run dev      # http://localhost:3000
npm run build
npm run lint

# Docker - Stack complète
docker compose up -d
docker compose logs -f
docker compose down
```

## Conventions du Projet

### Flux Event Sourcing
1. **Commande** → Le handler valide et appelle la méthode de l'agrégat
2. **Agrégat** → Produit des événements de domaine via `Apply(event)`
3. **EventRepository** → Persiste les événements avec versioning
4. **Channel** → Diffuse les événements au service en arrière-plan de projection
5. **ProjectionDispatcher** → Pattern matching des événements et mise à jour des modèles de lecture

**Exemple** : [CreateQsoAggregateCommandHandler.cs](backend/QsoManager.Application/Commands/QsoAggregate/CreateQsoAggregateCommandHandler.cs#L58-L76)

### Pattern Événement
```csharp
// Définir les événements comme des records imbriqués avec des noms au passé
public static class Events
{
    public record Created(Guid AggregateId, DateTime DateEvent, string Name, ...) : Event(AggregateId, DateEvent);
    public record ParticipantAdded(Guid AggregateId, DateTime DateEvent, string CallSign, int Order) : Event(AggregateId, DateEvent);
}

// Appliquer dans l'agrégat
protected override Validation<Error, Unit> When(IEvent @event)
{
    return @event switch
    {
        Events.Created e => HandleCreated(e),
        Events.ParticipantAdded e => HandleParticipantAdded(e),
        _ => Error.New($"Event type {@event.GetType().Name} not handled")
    };
}
```

### Pattern Commande
```csharp
// Records implémentant ICommand<TResult> avec ClaimsPrincipal pour l'authentification
public record CreateQsoAggregateCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Frequency,
    ClaimsPrincipal User
) : ICommand<QsoAggregateDto>;
```

### Mises à jour des Projections
Toujours mettre à jour le timestamp `UpdatedAt` et le dictionnaire `History` avec des descriptions en français :
```csharp
projection.Participants.Add(newParticipant);
projection.UpdatedAt = e.DateEvent;
projection.History.Add(e.DateEvent, $"Ajout du participant {e.CallSign}");
await _qsoProjectionRepository.SaveAsync(projection, cancellationToken);
```

### Pattern Contrôleur
```csharp
[HttpPost]
[Authorize]  // JWT requis
public async Task<ActionResult<QsoAggregateDto>> Create([FromBody] CreateRequest request)
{
    var command = new CreateCommand(request.Id ?? Guid.NewGuid(), request.Name, ..., User);
    var result = await _mediator.Send(command);
    return result.Match<ActionResult<QsoAggregateDto>>(
        dto => Ok(dto),
        errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
    );
}
```

## Points d'Intégration

### Mises à jour en Temps Réel via SignalR
- **Hub Backend** : [QsoHub.cs](backend/QsoManager.Api/Hubs/QsoHub.cs) avec notifications basées sur les groupes
- **Hook Frontend** : [useQsoSignalR.ts](frontend/src/hooks/useQsoSignalR.ts) gère le cycle de vie de la connexion
- **Pattern Groupe** : `qso_{qsoId}` pour les abonnements par QSO
- **Événements** : `participantAdded`, `participantRemoved`, `qsoOrderUpdated`, `qsoDeleted`

### APIs Externes
- **Lookups QRZ.com** : Enrichit les données des participants avec les infos d'indicatif, pays, DXCC
- **Cartes Leaflet** : [ParticipantMap.tsx](frontend/src/components/ParticipantMap.tsx) affiche les locators QTH géographiquement

### Authentification
- **Tokens JWT** : Générés à la connexion, stockés dans localStorage, attachés via intercepteur axios
- **MongoDB Identity** : [ApplicationUser](backend/QsoManager.Infrastructure/Identity/ApplicationUser.cs) avec AspNetCore.Identity.MongoDbCore
- **Routes Protégées** : [ProtectedRoute.tsx](frontend/src/components/ProtectedRoute.tsx) vérifie l'état d'authentification

## Sécurité

- **Autorisation** : Tous les endpoints d'écriture nécessitent l'attribut `[Authorize]`
- **Contexte Utilisateur** : Passer `ClaimsPrincipal User` du contrôleur aux commandes
- **Validation Modérateur** : Les commandes valident que le `ModeratorId` correspond à l'ID de l'utilisateur authentifié
- **CORS** : Configuré dans [Program.cs](backend/QsoManager.Api/Program.cs#L106-L115) pour l'origine frontend
- **Secrets** : Clés JWT stockées dans appsettings (utiliser User Secrets/variables d'environnement en production)

## Dépendances Clés

**Backend** : MediatR, LanguageExt, MongoDB.Driver, AspNetCore.Identity.MongoDbCore, SignalR  
**Frontend** : React 18, TypeScript, Vite, Axios, @microsoft/signalr, react-leaflet, @dnd-kit  
**Testing** : xUnit, FluentAssertions, NSubstitute, Verify.Xunit pour les tests snapshot

## Workflow Git

Ce projet utilise **GitFlow** avec commits en français obligatoires. Voir [skills/gitflow.md](skills/gitflow.md) pour les détails complets.
