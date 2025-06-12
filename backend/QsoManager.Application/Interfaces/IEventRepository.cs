using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Common;

namespace QsoManager.Application.Interfaces;

public interface IEventRepository
{
    Task<Validation<Error, IEnumerable<IEvent>>> GetAsync(Guid aggregateId, CancellationToken cancellationToken = default, int fromVersion = 0);
    Task<Validation<Error, IEnumerable<IEvent>>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<Validation<Error, long>> SaveEventsAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
}
