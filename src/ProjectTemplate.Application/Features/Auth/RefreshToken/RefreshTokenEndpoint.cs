using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Features.Auth.RefreshToken;

internal sealed class RefreshTokenEndpoint : IEndpoint
{
    public string[] Permissions => [];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh-token", async (
            [FromBody] RefreshTokenCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status401Unauthorized);
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .Produces<TokenResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
