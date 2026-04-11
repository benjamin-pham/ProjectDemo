using System.Security.Claims;
using MyProject.Application.Features.Auth.Shared;

namespace MyProject.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    TokenResponse GenerateToken(string sub, params IEnumerable<Claim> extraClaims);
    string HashToken(string token);
}
