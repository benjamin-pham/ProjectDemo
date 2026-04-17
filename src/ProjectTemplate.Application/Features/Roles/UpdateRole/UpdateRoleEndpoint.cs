using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;

namespace ProjectTemplate.Application.Features.Roles.UpdateRole;

internal sealed class UpdateRoleEndpoint : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/roles", async (
            [FromBody] UpdateRoleCommand request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(request, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description);
        })
        .RequireAuthorization()
        .WithName("UpdateRole")
        .WithTags("Roles")
        .Produces<UpdateRoleResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}

internal sealed record UpdateRoleRequest(
    string Name,
    string Description,
    string Type,
    List<string> Permissions);
