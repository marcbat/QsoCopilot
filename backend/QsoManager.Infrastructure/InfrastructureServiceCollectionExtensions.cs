using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Repositories;
using QsoManager.Infrastructure.Repositories;

namespace QsoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB
        services.AddSingleton<IMongoClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
            return new MongoClient(connectionString);
        });

        // Repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IQsoAggregateRepository, QsoAggregateRepository>();

        return services;
    }
}
