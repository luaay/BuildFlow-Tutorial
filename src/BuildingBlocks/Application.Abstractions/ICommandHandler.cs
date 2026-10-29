using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Command handler without a response value.
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

// Command handler with a response value.
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}