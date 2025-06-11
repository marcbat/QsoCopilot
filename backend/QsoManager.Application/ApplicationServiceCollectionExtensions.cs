using Microsoft.Extensions.DependencyInjection;
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

        return services;
    }
}
