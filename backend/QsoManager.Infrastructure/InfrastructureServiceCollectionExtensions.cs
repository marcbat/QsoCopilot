using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using AspNetCore.Identity.MongoDbCore.Extensions;
using QsoManager.Application.Interfaces;
using QsoManager.Application.Interfaces.Auth;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Domain.Repositories;
using QsoManager.Infrastructure.Authentication;
using QsoManager.Infrastructure.Identity;
using QsoManager.Infrastructure.Projections;
using QsoManager.Infrastructure.Repositories;

namespace QsoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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
            configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017",
            "QsoManagerDb")
        .AddDefaultTokenProviders();        // Authentication Service
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
