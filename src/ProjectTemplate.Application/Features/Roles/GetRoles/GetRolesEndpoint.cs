using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;
using ProjectTemplate.Application.Features.Users.GetUsers;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Features.Roles.GetRoles;


internal sealed class GetRolesEndpoint : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/roles", async (
            [AsParameters] GetRolesQuery request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(request, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status400BadRequest);
        })
        // .RequireAuthorization()
        .WithName("GetRoles")
        .WithTags("Roles")
        .Produces<PagedList<GetRolesResponse>>()
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
