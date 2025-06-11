using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QsoManager.Domain.Common;

namespace QsoManager.Application.Projections.Services;

public class ProjectionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProjectionHostedService> _logger;
    private readonly Channel<IEvent> _eventChannel;

    public ProjectionHostedService(
        IServiceProvider serviceProvider,
        ILogger<ProjectionHostedService> logger,
        Channel<IEvent> eventChannel)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventChannel = eventChannel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Projection hosted service started");

        await foreach (var @event in _eventChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<ProjectionDispatcherService>();
                
                var result = await dispatcher.DispatchAsync(@event, stoppingToken);
                result.Match(
                    success => _logger.LogInformation("Successfully dispatched event {EventType}", @event.GetType().Name),
                    error => _logger.LogError("Failed to dispatch event {EventType}: {Error}", @event.GetType().Name, error)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}", @event.GetType().Name);
            }
        }

        _logger.LogInformation("Projection hosted service stopped");
    }
}
