using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Users.AssignRolesToUser;

internal sealed class AssignRolesToUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/{id:guid}/roles", async (
            Guid id,
            AssignRolesToUserRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new AssignRolesToUserCommand(id, body.RoleIds);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code switch
                    {
                        "User.NotFound" => StatusCodes.Status404NotFound,
                        "Role.NotFound" => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    });
        })
        .RequireAuthorization()
        .WithName("AssignRolesToUser")
        .WithTags("Users")
        .Produces<AssignRolesToUserResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}

public sealed record AssignRolesToUserRequest(List<Guid> RoleIds);
