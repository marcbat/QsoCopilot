using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using QsoManager.Application.Interfaces;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Domain.Repositories;
using QsoManager.Infrastructure.Projections;
using QsoManager.Infrastructure.Repositories;

namespace QsoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {        // Configure MongoDB GUID representation - évite les conflits de sérialiseurs
        try
        {
            var guidSerializer = new GuidSerializer(GuidRepresentation.Standard);
            BsonSerializer.TryRegisterSerializer(guidSerializer);
        }
        catch (Exception ex)
        {
            // Log l'erreur si nécessaire, mais ne pas planter l'application
            Console.WriteLine($"Erreur lors de l'enregistrement du sérialiseur MongoDB: {ex.Message}");
        }
        
        // MongoDB
        services.AddSingleton<IMongoClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
            return new MongoClient(connectionString);
        });

        // Repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IQsoAggregateRepository, QsoAggregateRepository>();        // Projection repositories
        services.AddScoped<IQsoAggregateProjectionRepository, QsoManager.Infrastructure.Projections.QsoAggregateProjectionRepository>();
        services.AddScoped<IMigrationRepository, MigrationRepository>();

        return services;
    }
}
