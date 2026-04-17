using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;

namespace ProjectTemplate.Application.Features.Roles.GetRoleById;

internal sealed class GetRoleByIdEndpoint : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/roles/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRoleByIdQuery(id), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code == "Role.NotFound"
                        ? StatusCodes.Status404NotFound
                        : StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("GetRoleById")
        .WithTags("Roles")
        .Produces<RoleDetailResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
