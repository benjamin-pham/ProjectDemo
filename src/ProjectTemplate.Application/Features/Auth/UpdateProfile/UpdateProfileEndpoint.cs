using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;

namespace ProjectTemplate.Application.Features.Auth.UpdateProfile;

internal sealed class UpdateProfileEndpoint : IEndpoint
{
    public string[] Permissions => [];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/auth/me", async (
            [FromBody] UpdateProfileCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status400BadRequest);

        })
        .RequireAuthorization()
        .WithName("UpdateProfile")
        .WithTags("Auth")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
