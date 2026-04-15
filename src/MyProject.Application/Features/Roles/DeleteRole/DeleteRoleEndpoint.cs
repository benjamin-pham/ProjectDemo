using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;
using MyProject.Domain.Errors;

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
                ? Results.Ok()
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("DeleteRole")
        .WithTags("Roles")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
