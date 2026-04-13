using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Users.UpdateUser;

internal sealed class UpdateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/{id:guid}", async (
            UpdateUserCommand request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(request, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code == "User.NotFound"
                        ? StatusCodes.Status404NotFound
                        : StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("UpdateUser")
        .WithTags("Users")
        .Produces<UpdateUserResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
