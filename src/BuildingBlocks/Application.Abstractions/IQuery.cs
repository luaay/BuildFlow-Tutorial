using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Read query — always returns data of type TResponse
// No "no-response" version because a query inherently returns data
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}