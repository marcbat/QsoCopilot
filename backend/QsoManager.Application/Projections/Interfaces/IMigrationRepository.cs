using LanguageExt;
using LanguageExt.Common;

namespace QsoManager.Application.Projections.Interfaces;

public interface IMigrationRepository
{
    Task<Validation<Error, Unit>> ResetProjectionsAsync(CancellationToken cancellationToken = default);
}
