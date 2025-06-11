using LanguageExt;
using LanguageExt.Common;
using MediatR;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands;

public interface ICommand<TResult> : IRequest<Validation<Error, TResult>>
{
}

public interface ICommand : IRequest<Validation<Error, LanguageExt.Unit>>
{
}

public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, Validation<Error, TResult>>
    where TCommand : ICommand<TResult>
{
}

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Validation<Error, LanguageExt.Unit>>
    where TCommand : ICommand
{
}
