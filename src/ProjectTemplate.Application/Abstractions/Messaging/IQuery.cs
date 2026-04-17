using MediatR;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
