using MediatR;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Abstractions.Messaging;

public interface IBaseCommand;

public interface ICommand : IRequest<Result>, IBaseCommand
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}
