using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyProject.Application.Abstractions.Endpoints;

namespace MyProject.Application.Features.Auth.UpdateProfile;

internal sealed class UpdateProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/auth/me", async (
            UpdateProfileCommand command,
            ISender sender,
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
