using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Common;
using System.Text.Json;

namespace QsoManager.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<EventRepository> _logger;
    private readonly string? _databaseName;

    public EventRepository(IMongoClient mongoClient, IConfiguration configuration, ILogger<EventRepository> logger)
    {
        _mongoClient = mongoClient;
        _logger = logger;
        _databaseName = configuration["Mongo:Database"];
    }

    public async Task<Validation<Error, IEnumerable<IEvent>>> GetAsync(Guid aggregateId, CancellationToken cancellationToken = default, int fromVersion = 0)
    {
        try
        {
            var database = _mongoClient.GetDatabase(_databaseName);
            var collection = database.GetCollection<EventData>("Events");
            
            var filterBuilder = Builders<EventData>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(x => x.AggregateId, aggregateId),
                filterBuilder.Gte(x => x.Version, fromVersion)
            );

            var sort = Builders<EventData>.Sort.Ascending(x => x.Version);
            var result = await collection
                .Find(filter)
                .Sort(sort)
                .ToListAsync(cancellationToken);

            var events = result.Select(x => x.PayLoad);
            return Validation<Error, IEnumerable<IEvent>>.Success(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible de récupérer les événements de l'agrégat {AggregateId}.", aggregateId);
            return Error.New($"Impossible de récupérer les événements de l'agrégat {aggregateId}.", Error.New(ex.Message));
        }
    }

    public async Task<Validation<Error, long>> SaveEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!events.Any())
                return 0L;

            var database = _mongoClient.GetDatabase(_databaseName);
            var collection = database.GetCollection<EventData>("Events");
            var bulkOps = new List<WriteModel<EventData>>();

            foreach (var @event in events)
            {
                var eventData = new EventData(
                    Guid.NewGuid(), 
                    @event.AggregateId, 
                    @event.Version, 
                    @event, 
                    DateTime.UtcNow, 
                    @event.GetType().AssemblyQualifiedName!
                );
                bulkOps.Add(new InsertOneModel<EventData>(eventData));
            }

            var result = await collection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
            return result.InsertedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible de sauvegarder les événements.");
            return Error.New("Impossible de sauvegarder les événements.", Error.New(ex.Message));
        }
    }    public class EventData
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; private set; }

        [BsonElement("aggregateId")]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid AggregateId { get; private set; }

        [BsonElement("version")]
        public int Version { get; private set; }

        [BsonElement("payload")]
        public IEvent PayLoad { get; private set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; private set; }

        [BsonElement("eventType")]
        public string EventType { get; private set; }

        public EventData(Guid id, Guid aggregateId, int version, IEvent payLoad, DateTime timestamp, string eventType)
        {
            Id = id;
            AggregateId = aggregateId;
            Version = version;
            PayLoad = payLoad;
            Timestamp = timestamp;
            EventType = eventType;
        }
    }

    public async Task<Validation<Error, IEnumerable<IEvent>>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _mongoClient.GetDatabase(_databaseName);
            var collection = database.GetCollection<EventData>("Events");
            
            var sort = Builders<EventData>.Sort.Ascending(x => x.Timestamp).Ascending(x => x.Version);
            var result = await collection
                .Find(_ => true)
                .Sort(sort)
                .ToListAsync(cancellationToken);

            var events = result.Select(x => x.PayLoad);
            return Validation<Error, IEnumerable<IEvent>>.Success(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible de récupérer tous les événements.");
            return Error.New("Impossible de récupérer tous les événements.", Error.New(ex.Message));
        }
    }
}
