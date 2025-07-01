using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Domain.Common;
using System.Threading.Channels;

namespace QsoManager.Application;

public abstract class BaseCommandHandler<T>
{
    private readonly Channel<IEvent> _channel;
    protected readonly ILogger<T> _logger;

    protected BaseCommandHandler(Channel<IEvent> channel, ILogger<T> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    protected virtual Validation<Error, IEnumerable<IEvent>> DispatchEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var @event in events)
        {
            _channel.Writer.WriteAsync(@event, cancellationToken);
        }

        return Validation<Error, IEnumerable<IEvent>>.Success(events);
    }
}
