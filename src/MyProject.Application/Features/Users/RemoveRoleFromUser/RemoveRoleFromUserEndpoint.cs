using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Users.RemoveRoleFromUser;

internal sealed class RemoveRoleFromUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/users/{id:guid}/roles/{roleId:guid}", async (
            Guid id,
            Guid roleId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RemoveRoleFromUserCommand(id, roleId), ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code switch
                    {
                        "User.NotFound" => StatusCodes.Status404NotFound,
                        "Role.NotAssigned" => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status400BadRequest
                    });
        })
        .RequireAuthorization()
        .WithName("RemoveRoleFromUser")
        .WithTags("Users")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
