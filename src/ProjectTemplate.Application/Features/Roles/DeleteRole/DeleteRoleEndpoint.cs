using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;
using ProjectTemplate.Domain.Errors;

namespace ProjectTemplate.Application.Features.Roles.DeleteRole;

internal sealed class DeleteRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/role/{id:guid}", async (
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
                    statusCode: StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("DeleteRole")
        .WithTags("Roles")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
