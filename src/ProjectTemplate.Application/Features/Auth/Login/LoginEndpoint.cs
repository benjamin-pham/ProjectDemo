using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProjectTemplate.Application.Abstractions.Endpoints;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Features.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint
{
    public string[] Permissions => [];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            [FromBody] LoginUserCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: StatusCodes.Status400BadRequest);
        })
        .WithName("Login")
        .WithTags("Auth")
        .Produces<TokenResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
