using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Features.Auth.Shared;

namespace MyProject.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptionsMonitor<JwtSettings> options) : IJwtTokenService
{
    private JwtSettings Settings => options.CurrentValue;

    public TokenResponse GenerateToken(string sub, params IEnumerable<Claim> extraClaims)
    {
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(Settings.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(Settings.RefreshTokenExpirationDays);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, sub),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        }.Concat(extraClaims ?? []);

        var jwtToken = new JwtSecurityToken(
            issuer: Settings.Issuer,
            audience: Settings.Audience,
            claims: claims,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenResponse(accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt, "Bearer");
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
