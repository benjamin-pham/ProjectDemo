using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Roles.UpdateRole;

internal sealed class UpdateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/roles/{id:guid}", async (
            Guid id,
            UpdateRoleRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateRoleCommand(
                id,
                body.Name,
                body.Description,
                body.Type,
                body.Permissions);

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code switch
                    {
                        "Role.NotFound" => StatusCodes.Status404NotFound,
                        "Role.NameAlreadyTaken" => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status400BadRequest
                    });
        })
        .RequireAuthorization()
        .WithName("UpdateRole")
        .WithTags("Roles")
        .Produces<UpdateRoleResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}

internal sealed record UpdateRoleRequest(
    string Name,
    string Description,
    string Type,
    List<string> Permissions);
