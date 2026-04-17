using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public Guid UserId =>
        contextAccessor
            .HttpContext?.User
            .GetUserId() ?? throw new UnauthorizedAccessException("User context is unavailable");

    public bool IsAuthenticated => contextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? userId =
            principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userId, out Guid parsedUserId)
            ? parsedUserId
            : throw new UnauthorizedAccessException("User identifier is unavailable");
    }
}
