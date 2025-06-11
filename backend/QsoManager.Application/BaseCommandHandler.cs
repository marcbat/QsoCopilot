using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Common;
using System.Threading.Channels;

namespace QsoManager.Application;

public abstract class BaseCommandHandler
{
    private readonly Channel<IEvent> _channel;

    protected BaseCommandHandler(Channel<IEvent> channel)
    {
        _channel = channel;
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
