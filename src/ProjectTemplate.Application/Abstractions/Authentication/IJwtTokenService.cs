using System.Security.Claims;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    TokenResponse GenerateToken(string sub, params IEnumerable<Claim> extraClaims);
    string HashToken(string token);
}
