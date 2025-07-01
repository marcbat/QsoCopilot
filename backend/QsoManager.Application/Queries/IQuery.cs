using LanguageExt;
using LanguageExt.Common;
using MediatR;

namespace QsoManager.Application;

public interface IQuery<TResult> : IRequest<Validation<Error, TResult>>
{
}

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, Validation<Error, TResult>>
    where TQuery : IQuery<TResult>
{
}
