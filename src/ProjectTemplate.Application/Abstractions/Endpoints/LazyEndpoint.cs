using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Abstractions.Endpoints;

public abstract class LazyEndpoint<TRequest, TResponse> : IEndpoint where TRequest : IRequest<Result<TResponse>>
{
    public virtual void MapEndpoint(IEndpointRouteBuilder app)
    {
        var endpoint = app.MapPost(Route, async (
            [FromBody] TRequest command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                if (result.IsFailure)
                {
                    return Results.Problem(
                        title: result.Error.Code,
                        detail: result.Error.Description,
                        statusCode: StatusCodes.Status400BadRequest);
                }

                return Results.Ok(result);
            })
        .WithName(Name)
        .WithTags(Tag)
        .Produces(StatusCodes.Status200OK);

        if (IsRequireAuth)
        {
            endpoint.RequireAuthorization(Permissions)
                .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
        }
    }
    protected abstract string[] Permissions { get; }
    protected abstract string Route { get; }
    protected virtual string Name => typeof(TRequest).Name.Replace("Command", "").Replace("Query", "");
    protected abstract string Tag { get; }
    protected abstract bool IsRequireAuth { get; }
}

public abstract class LazyEndpoint<TRequest> : IEndpoint where TRequest : IRequest<Result>
{
    public virtual void MapEndpoint(IEndpointRouteBuilder app)
    {
        var endpoint = app.MapPost(Route, async (
            [FromBody] TRequest command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                if (result.IsFailure)
                {
                    return Results.Problem(
                        title: result.Error.Code,
                        detail: result.Error.Description,
                        statusCode: StatusCodes.Status400BadRequest);
                }

                return Results.Ok();
            })
        .WithName(Name)
        .WithTags(Tag)
        .Produces(StatusCodes.Status200OK);

        if (IsRequireAuth)
        {
            endpoint.RequireAuthorization(Permissions)
                .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
        }
    }
    protected abstract string[] Permissions { get; }
    protected abstract string Route { get; }
    protected virtual string Name => typeof(TRequest).Name.Replace("Command", "").Replace("Query", "");
    protected virtual string Tag => "Default";
    protected abstract bool IsRequireAuth { get; }
}