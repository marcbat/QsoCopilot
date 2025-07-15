using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QsoManager.Application.Common;
using QsoManager.Application.Projections.Interfaces;
using System.Text.RegularExpressions;
using ApplicationModels = QsoManager.Application.Projections.Models;
using InfrastructureModels = QsoManager.Infrastructure.Projections.Models;
using static LanguageExt.Prelude;

namespace QsoManager.Infrastructure.Projections;

public class QsoAggregateProjectionRepository : IQsoAggregateProjectionRepository
{
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<QsoAggregateProjectionRepository> _logger;
    private readonly string _databaseName;
    private readonly string _collectionName = "QsoAggregateProjections";

    public QsoAggregateProjectionRepository(
        IMongoClient mongoClient,
        IConfiguration configuration,
        ILogger<QsoAggregateProjectionRepository> logger)
    {
        _mongoClient = mongoClient;
        _logger = logger;
        _databaseName = configuration["Mongo:Database"] ?? "QsoManager";
    }

    private IMongoCollection<InfrastructureModels.QsoAggregateProjection> GetCollection()
    {
        var database = _mongoClient.GetDatabase(_databaseName);
        return database.GetCollection<InfrastructureModels.QsoAggregateProjection>(_collectionName);
    }

    public async Task<Validation<Error, ApplicationModels.QsoAggregateProjectionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Eq(x => x.Id, id);
            var result = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return Error.New($"QsoAggregate projection with ID {id} not found");

            return MapToDto(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving QsoAggregate projection {Id}", id);
            return Error.New($"Failed to retrieve QsoAggregate projection {id}: {ex.Message}");
        }
    }

    public async Task<Validation<Error, IEnumerable<ApplicationModels.QsoAggregateProjectionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var results = await collection.Find(_ => true).ToListAsync(cancellationToken);

            return Success<Error, IEnumerable<ApplicationModels.QsoAggregateProjectionDto>>(results.Select(MapToDto).AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all QsoAggregate projections");
            return Error.New($"Failed to retrieve QsoAggregate projections: {ex.Message}");
        }
    }

    public async Task<Validation<Error, Unit>> SaveAsync(ApplicationModels.QsoAggregateProjectionDto entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var model = MapToModel(entity);
            await collection.InsertOneAsync(model, cancellationToken: cancellationToken);

            _logger.LogDebug("QsoAggregate projection saved successfully with ID {Id}", entity.Id);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving QsoAggregate projection {Id}", entity.Id);
            return Error.New($"Failed to save QsoAggregate projection {entity.Id}: {ex.Message}");
        }
    }

    public async Task<Validation<Error, Unit>> UpdateAsync(Guid id, ApplicationModels.QsoAggregateProjectionDto entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Eq(x => x.Id, id);
            var model = MapToModel(entity);
            
            var result = await collection.ReplaceOneAsync(filter, model, cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
                return Error.New($"QsoAggregate projection with ID {id} not found for update");

            _logger.LogDebug("QsoAggregate projection updated successfully with ID {Id}", id);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating QsoAggregate projection {Id}", id);
            return Error.New($"Failed to update QsoAggregate projection {id}: {ex.Message}");
        }
    }

    public async Task<Validation<Error, Unit>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Eq(x => x.Id, id);
            var result = await collection.DeleteOneAsync(filter, cancellationToken);

            if (result.DeletedCount == 0)
                return Error.New($"QsoAggregate projection with ID {id} not found for deletion");

            _logger.LogDebug("QsoAggregate projection deleted successfully with ID {Id}", id);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting QsoAggregate projection {Id}", id);
            return Error.New($"Failed to delete QsoAggregate projection {id}: {ex.Message}");
        }
    }

    public async Task<Validation<Error, Unit>> DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            await collection.DeleteManyAsync(_ => true, cancellationToken);

            _logger.LogDebug("All QsoAggregate projections deleted successfully");
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all QsoAggregate projections");
            return Error.New($"Failed to delete all QsoAggregate projections: {ex.Message}");
        }
    }    public async Task<Validation<Error, IEnumerable<ApplicationModels.QsoAggregateProjectionDto>>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            
            _logger.LogInformation("Searching QSO projections containing '{SearchTerm}'", name);
            
            // Détecter si nous utilisons Cosmos DB
            var isCosmosDb = await IsCosmosDbAsync();
            
            FilterDefinition<InfrastructureModels.QsoAggregateProjection> filter;
            
            if (isCosmosDb)
            {
                // Cosmos DB: Utiliser une recherche simple sans regex complexe
                _logger.LogDebug("Using Cosmos DB compatible search for '{SearchTerm}'", name);
                
                // Recherche par égalité partielle avec StartsWith ou Contains
                // Note: Cosmos DB supporte mieux les comparaisons simples
                var startsWithFilter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Regex(
                    x => x.Name, 
                    new MongoDB.Bson.BsonRegularExpression($"^{Regex.Escape(name)}", "i"));
                
                var containsFilter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Regex(
                    x => x.Name, 
                    new MongoDB.Bson.BsonRegularExpression(Regex.Escape(name), "i"));
                
                // Utiliser une recherche par contains plus simple
                filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Or(startsWithFilter, containsFilter);
            }
            else
            {
                // MongoDB natif: Utiliser des regex complètes
                _logger.LogDebug("Using MongoDB native regex search for '{SearchTerm}'", name);
                
                var safeSearchTerm = name.Replace("\\", "\\\\").Replace(".", "\\.");
                var regexPattern = $".*{safeSearchTerm}.*";
                
                filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Regex(
                    x => x.Name, 
                    new MongoDB.Bson.BsonRegularExpression(regexPattern, "i"));
            }
            
            var results = await collection.Find(filter).ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} QSO projections containing '{SearchTerm}'", 
                results.Count, name);
            
            return Success<Error, IEnumerable<ApplicationModels.QsoAggregateProjectionDto>>(results.Select(MapToDto).AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching QsoAggregate projections by name '{Name}'", name);
            return Error.New($"Failed to search QsoAggregate projections by name '{name}': {ex.Message}");
        }
    }

    /// <summary>
    /// Détecte si nous utilisons Azure Cosmos DB
    /// </summary>
    private async Task<bool> IsCosmosDbAsync()
    {
        try
        {
            // Vérifier les variables d'environnement ou la configuration
            var cosmosDbIndicator = Environment.GetEnvironmentVariable("COSMOS_DB_ENABLED");
            if (!string.IsNullOrEmpty(cosmosDbIndicator) && 
                (cosmosDbIndicator.ToLower() == "true" || cosmosDbIndicator == "1"))
            {
                return true;
            }

            // Méthode alternative: tenter une commande MongoDB spécifique
            var database = _mongoClient.GetDatabase(_databaseName);
            var command = new MongoDB.Bson.BsonDocument("buildInfo", 1);
            var result = await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(command);
            
            if (result.Contains("version"))
            {
                var version = result["version"].AsString;
                return version.ToLower().Contains("cosmos");
            }
            
            return false;
        }
        catch
        {
            // En cas d'erreur, assumer MongoDB natif
            return false;
        }
    }

    public async Task<Validation<Error, IEnumerable<ApplicationModels.QsoAggregateProjectionDto>>> SearchByModeratorAsync(Guid moderatorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            
            _logger.LogInformation("Searching QSO projections moderated by user '{ModeratorId}'", moderatorId);
            
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Eq(x => x.ModeratorId, moderatorId);
            
            var results = await collection.Find(filter).ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} QSO projections moderated by user '{ModeratorId}'", 
                results.Count, moderatorId);
            
            return Success<Error, IEnumerable<ApplicationModels.QsoAggregateProjectionDto>>(results.Select(MapToDto).AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching QsoAggregate projections by moderator '{ModeratorId}'", moderatorId);
            return Error.New($"Failed to search QsoAggregate projections by moderator '{moderatorId}': {ex.Message}");        }
    }

    public async Task<Validation<Error, PagedResult<ApplicationModels.QsoAggregateProjectionDto>>> GetAllPaginatedAsync(PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!pagination.IsValid)
                return Error.New("Invalid pagination parameters");

            var collection = GetCollection();
            
            // Compter le total d'éléments
            var totalCount = await collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
              // Récupérer les éléments paginés triés par date de création décroissante (plus récent en premier)
            var results = await collection
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .Skip(pagination.Skip)
                .Limit(pagination.PageSize)
                .ToListAsync(cancellationToken);

            var mappedResults = results.Select(MapToDto);
            var pagedResult = new PagedResult<ApplicationModels.QsoAggregateProjectionDto>(
                mappedResults, totalCount, pagination.PageNumber, pagination.PageSize);

            _logger.LogInformation("Retrieved page {PageNumber} of {TotalPages} with {ItemCount} items (total: {TotalCount})",
                pagedResult.PageNumber, pagedResult.TotalPages, results.Count, totalCount);

            return Success<Error, PagedResult<ApplicationModels.QsoAggregateProjectionDto>>(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated QsoAggregate projections");
            return Error.New($"Failed to retrieve paginated QsoAggregate projections: {ex.Message}");
        }
    }

    public async Task<Validation<Error, PagedResult<ApplicationModels.QsoAggregateProjectionDto>>> SearchByNamePaginatedAsync(string name, PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!pagination.IsValid)
                return Error.New("Invalid pagination parameters");

            var collection = GetCollection();
            
            _logger.LogInformation("Searching paginated QSO projections containing '{SearchTerm}' (page {PageNumber}, size {PageSize})", 
                name, pagination.PageNumber, pagination.PageSize);
            
            // Créer un pattern regex pour recherche partielle case-insensitive
            var safeSearchTerm = name.Replace("\\", "\\\\").Replace(".", "\\.");
            var regexPattern = $".*{safeSearchTerm}.*";
            
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Regex(
                x => x.Name, 
                new MongoDB.Bson.BsonRegularExpression(regexPattern, "i"));
              // Compter le total d'éléments correspondant au filtre
            var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            
            // Récupérer les éléments paginés triés par date de création décroissante (plus récent en premier)
            var results = await collection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Skip(pagination.Skip)
                .Limit(pagination.PageSize)
                .ToListAsync(cancellationToken);
            
            var mappedResults = results.Select(MapToDto);
            var pagedResult = new PagedResult<ApplicationModels.QsoAggregateProjectionDto>(
                mappedResults, totalCount, pagination.PageNumber, pagination.PageSize);

            _logger.LogInformation("Found page {PageNumber} of {TotalPages} with {ItemCount} items containing '{SearchTerm}' (total: {TotalCount})", 
                pagedResult.PageNumber, pagedResult.TotalPages, results.Count, name, totalCount);
            
            return Success<Error, PagedResult<ApplicationModels.QsoAggregateProjectionDto>>(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching paginated QsoAggregate projections by name '{Name}'", name);
            return Error.New($"Failed to search paginated QsoAggregate projections by name '{name}': {ex.Message}");
        }
    }

    public async Task<Validation<Error, PagedResult<ApplicationModels.QsoAggregateProjectionDto>>> SearchByModeratorPaginatedAsync(Guid moderatorId, PaginationParameters pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!pagination.IsValid)
                return Error.New("Invalid pagination parameters");

            var collection = GetCollection();
            
            _logger.LogInformation("Searching paginated QSO projections moderated by user '{ModeratorId}' (page {PageNumber}, size {PageSize})", 
                moderatorId, pagination.PageNumber, pagination.PageSize);
            
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Eq(x => x.ModeratorId, moderatorId);
            
            // Compter le total d'éléments correspondant au filtre
            var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
              // Récupérer les éléments paginés triés par date de création décroissante (plus récent en premier)
            var results = await collection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Skip(pagination.Skip)
                .Limit(pagination.PageSize)
                .ToListAsync(cancellationToken);
            
            var mappedResults = results.Select(MapToDto);
            var pagedResult = new PagedResult<ApplicationModels.QsoAggregateProjectionDto>(
                mappedResults, totalCount, pagination.PageNumber, pagination.PageSize);

            _logger.LogInformation("Found page {PageNumber} of {TotalPages} with {ItemCount} items moderated by user '{ModeratorId}' (total: {TotalCount})", 
                pagedResult.PageNumber, pagedResult.TotalPages, results.Count, moderatorId, totalCount);
            
            return Success<Error, PagedResult<ApplicationModels.QsoAggregateProjectionDto>>(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching paginated QsoAggregate projections by moderator '{ModeratorId}'", moderatorId);
            return Error.New($"Failed to search paginated QsoAggregate projections by moderator '{moderatorId}': {ex.Message}");
        }
    }

    public async Task<Validation<Error, bool>> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<InfrastructureModels.QsoAggregateProjection>.Filter.Eq(x => x.Name, name);
            var count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if QsoAggregate projection exists with name {Name}", name);
            return Error.New($"Failed to check existence of QsoAggregate projection with name '{name}': {ex.Message}");
        }
    }    private ApplicationModels.QsoAggregateProjectionDto MapToDto(InfrastructureModels.QsoAggregateProjection model)
    {
        return new ApplicationModels.QsoAggregateProjectionDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ModeratorId = model.ModeratorId,
            Frequency = model.Frequency,
            StartDateTime = model.StartDateTime,
            Participants = model.Participants.Select(MapParticipantToDto).ToList(),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            History = ConvertHistoryToDto(model.History)
        };
    }

    private Dictionary<DateTime, string> ConvertHistoryToDto(Dictionary<string, string> infraHistory)
    {
        if (infraHistory == null) return new Dictionary<DateTime, string>();
        
        var result = new Dictionary<DateTime, string>();
        foreach (var entry in infraHistory)
        {
            if (DateTime.TryParse(entry.Key, out var date))
            {
                result[date] = entry.Value;
            }
        }
        return result;
    }

    private ApplicationModels.ParticipantProjectionDto MapParticipantToDto(InfrastructureModels.ParticipantProjection model)
    {
        return new ApplicationModels.ParticipantProjectionDto
        {
            CallSign = model.CallSign,
            Order = model.Order,
            AddedAt = model.AddedAt
        };
    }    private InfrastructureModels.QsoAggregateProjection MapToModel(ApplicationModels.QsoAggregateProjectionDto dto)
    {
        return new InfrastructureModels.QsoAggregateProjection
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ModeratorId = dto.ModeratorId,
            Frequency = dto.Frequency,
            StartDateTime = dto.StartDateTime,
            Participants = dto.Participants.Select(MapParticipantToModel).ToList(),
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            History = ConvertHistoryToModel(dto.History)
        };
    }

    private Dictionary<string, string> ConvertHistoryToModel(Dictionary<DateTime, string> dtoHistory)
    {
        if (dtoHistory == null) return new Dictionary<string, string>();
        
        var result = new Dictionary<string, string>();
        foreach (var entry in dtoHistory)
        {
            result[entry.Key.ToString("O")] = entry.Value; // Format ISO 8601
        }
        return result;
    }

    private InfrastructureModels.ParticipantProjection MapParticipantToModel(ApplicationModels.ParticipantProjectionDto dto)
    {
        return new InfrastructureModels.ParticipantProjection
        {
            CallSign = dto.CallSign,
            Order = dto.Order,
            AddedAt = dto.AddedAt
        };
    }
}
