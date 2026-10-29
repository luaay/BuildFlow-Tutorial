using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Query handler — matches IQuery<TResponse>
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}