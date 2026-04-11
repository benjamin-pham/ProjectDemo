namespace MyProject.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; init; } = 1440;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
