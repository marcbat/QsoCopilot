using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Application.Projections.Services;
using QsoManager.Application.Services;
using QsoManager.Domain.Common;
using System.Reflection;
using System.Threading.Channels;

namespace QsoManager.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Enregistrement de MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Enregistrement du Channel pour les événements
        services.AddSingleton(Channel.CreateUnbounded<IEvent>());

        // Services de projection
        services.AddScoped<IReprojectionService, ReprojectionService>();
        services.AddScoped<ProjectionDispatcherService>();
        
        // Services d'enrichissement
        services.AddScoped<IParticipantEnrichmentService, ParticipantEnrichmentService>();
        
        // Background service for automatic projection processing
        services.AddHostedService<QsoManager.Application.Projections.Services.ProjectionHostedService>();

        return services;
    }
}
