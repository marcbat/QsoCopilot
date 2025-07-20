using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using AspNetCore.Identity.MongoDbCore.Extensions;
using QsoManager.Application.Interfaces;
using QsoManager.Application.Interfaces.Auth;
using QsoManager.Application.Interfaces.Services;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Domain.Repositories;
using QsoManager.Infrastructure.Authentication;
using QsoManager.Infrastructure.Identity;
using QsoManager.Infrastructure.Projections;
using QsoManager.Infrastructure.Repositories;
using QsoManager.Infrastructure.Services;
using QsoManager.Infrastructure.Services.QRZ;

namespace QsoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Get MongoDB connection string
        var mongoConnectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var mongoDatabaseName = configuration["Mongo:Database"] ?? "QsoManager";
        
        // MongoDB
        services.AddSingleton<IMongoClient>(provider =>
        {
            return new MongoClient(mongoConnectionString);
        });

        // Service d'initialisation de la base de données
        services.AddHostedService<QsoManager.Infrastructure.Configuration.DatabaseInitializationService>();

        // Repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IQsoAggregateRepository, QsoAggregateRepository>();
        services.AddScoped<IModeratorAggregateRepository, ModeratorAggregateRepository>();

        // Projection repositories
        services.AddScoped<IQsoAggregateProjectionRepository, QsoManager.Infrastructure.Projections.QsoAggregateProjectionRepository>();
        services.AddScoped<IMigrationRepository, MigrationRepository>();        // Identity and Authentication with MongoDB
        services.AddIdentity<ApplicationUser, ApplicationRole>(identity =>
        {
            // Password settings
            identity.Password.RequireDigit = true;
            identity.Password.RequireLowercase = true;
            identity.Password.RequireNonAlphanumeric = true;
            identity.Password.RequireUppercase = true;
            identity.Password.RequiredLength = 6;
            identity.Password.RequiredUniqueChars = 1;

            // Lockout settings
            identity.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            identity.Lockout.MaxFailedAccessAttempts = 5;
            identity.Lockout.AllowedForNewUsers = true;

            // User settings
            identity.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            identity.User.RequireUniqueEmail = true;
        })
        .AddMongoDbStores<ApplicationUser, ApplicationRole, string>(
            mongoConnectionString,
            mongoDatabaseName)
        .AddDefaultTokenProviders();        // Authentication Service
        services.AddScoped<IAuthenticationService, AuthenticationService>();        // External Services
        services.AddHttpClient();
        services.AddSingleton<IQrzSessionCacheService, QrzSessionCacheService>();
        services.AddScoped<IQrzService, QrzService>();

        // Services de chiffrement
        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        return services;
    }
}
