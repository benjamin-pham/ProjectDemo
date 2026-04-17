using System;

namespace ProjectTemplate.Application.Features.Auth.Shared;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    string TokenType);