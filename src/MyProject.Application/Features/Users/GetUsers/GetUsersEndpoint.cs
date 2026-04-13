using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Users.GetUsers;

internal sealed class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetUsersQuery(
                Page: page > 0 ? page : 1,
                PageSize: pageSize is > 0 and <= 100 ? pageSize : 20);

            var result = await sender.Send(query, ct);

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
        .Produces<PagedResponse<UserListItemResponse>>()
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
