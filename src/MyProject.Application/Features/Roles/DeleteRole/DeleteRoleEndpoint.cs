using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Roles.DeleteRole;

internal sealed class DeleteRoleEndpoint : IEndpoint
{
    public string[] Permissions => [];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/roles/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteRoleCommand(id), ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code switch
                    {
                        "Role.NotFound" => StatusCodes.Status404NotFound,
                        "Role.HasActiveAssignments" => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status400BadRequest
                    });
        })
        .RequireAuthorization()
        .WithName("DeleteRole")
        .WithTags("Roles")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
