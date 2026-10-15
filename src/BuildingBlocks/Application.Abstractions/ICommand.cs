using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Command without a response value: returns success or failure.
public interface ICommand : IRequest<Result>
{
}

// Command with a response value: returns a result containing that value on success.
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}