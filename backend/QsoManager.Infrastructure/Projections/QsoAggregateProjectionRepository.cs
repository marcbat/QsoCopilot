using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Application.Projections.Models;
using QsoManager.Infrastructure.Projections.Models;
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
        _databaseName = configuration["Mongo:Database"] ?? "QsoManagerProjections";
    }

    private IMongoCollection<QsoAggregateProjection> GetCollection()
    {
        var database = _mongoClient.GetDatabase(_databaseName);
        return database.GetCollection<QsoAggregateProjection>(_collectionName);
    }    public async Task<Validation<Error, QsoAggregateProjectionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<QsoAggregateProjection>.Filter.Eq(x => x.Id, id);
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
    }    public async Task<Validation<Error, IEnumerable<QsoAggregateProjectionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {        try
        {
            var collection = GetCollection();
            var results = await collection.Find(_ => true).ToListAsync(cancellationToken);
            return Success<Error, IEnumerable<QsoAggregateProjectionDto>>(results.Select(MapToDto).AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all QsoAggregate projections");
            return Error.New($"Failed to retrieve QsoAggregate projections: {ex.Message}");
        }
    }    public async Task<Validation<Error, Unit>> SaveAsync(QsoAggregateProjectionDto entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var model = MapToModel(entity);
            await collection.InsertOneAsync(model, cancellationToken: cancellationToken);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving QsoAggregate projection {Id}", entity.Id);
            return Error.New($"Failed to save QsoAggregate projection {entity.Id}: {ex.Message}");
        }
    }    public async Task<Validation<Error, Unit>> UpdateAsync(Guid id, QsoAggregateProjectionDto entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<QsoAggregateProjection>.Filter.Eq(x => x.Id, id);
            var model = MapToModel(entity);
            var result = await collection.ReplaceOneAsync(filter, model, cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
                return Error.New($"QsoAggregate projection with ID {id} not found for update");

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
            var filter = Builders<QsoAggregateProjection>.Filter.Eq(x => x.Id, id);
            var result = await collection.DeleteOneAsync(filter, cancellationToken);

            if (result.DeletedCount == 0)
                return Error.New($"QsoAggregate projection with ID {id} not found for deletion");

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
            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all QsoAggregate projections");
            return Error.New($"Failed to delete all QsoAggregate projections: {ex.Message}");
        }
    }    public async Task<Validation<Error, IEnumerable<QsoAggregateProjectionDto>>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {        try
        {
            var collection = GetCollection();
            var filter = Builders<QsoAggregateProjection>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"));
            var results = await collection.Find(filter).ToListAsync(cancellationToken);
            return Success<Error, IEnumerable<QsoAggregateProjectionDto>>(results.Select(MapToDto).AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching QsoAggregate projections by name {Name}", name);
            return Error.New($"Failed to search QsoAggregate projections by name {name}: {ex.Message}");
        }
    }

    public async Task<Validation<Error, bool>> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<QsoAggregateProjection>.Filter.Eq(x => x.Name, name);
            var count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if QsoAggregate projection exists with name {Name}", name);
            return Error.New($"Failed to check QsoAggregate projection existence for name {name}: {ex.Message}");        }
    }

    private QsoAggregateProjectionDto MapToDto(QsoAggregateProjection model)
    {
        return new QsoAggregateProjectionDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ModeratorId = model.ModeratorId,
            Participants = model.Participants.Select(MapParticipantToDto).ToList(),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    private ParticipantProjectionDto MapParticipantToDto(ParticipantProjection model)
    {
        return new ParticipantProjectionDto
        {
            CallSign = model.CallSign,
            Order = model.Order,
            AddedAt = model.AddedAt
        };
    }

    private QsoAggregateProjection MapToModel(QsoAggregateProjectionDto dto)
    {
        return new QsoAggregateProjection
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ModeratorId = dto.ModeratorId,
            Participants = dto.Participants.Select(MapParticipantToModel).ToList(),
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private ParticipantProjection MapParticipantToModel(ParticipantProjectionDto dto)
    {
        return new ParticipantProjection
        {
            CallSign = dto.CallSign,
            Order = dto.Order,
            AddedAt = dto.AddedAt
        };
    }
}
