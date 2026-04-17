using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Features.Users.GetUsers;

internal sealed class GetUsersEndpoint : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (
            [AsParameters] GetUsersQuery request,
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
        .RequireAuthorization()
        .WithName("GetUsers")
        .WithTags("Users")
        .Produces<PagedList<GetUsersResponse>>()
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
