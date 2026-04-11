using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MyProject.Application.Features.Auth.Shared;

namespace MyProject.Application.Features.Auth.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger)
    : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    private static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Refresh token không hợp lệ hoặc đã hết hạn.");

    public async Task<Result<TokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var hashedToken = jwtTokenService.HashToken(request.RefreshToken);

        var user = await userRepository.GetByHashedRefreshTokenAsync(hashedToken, cancellationToken);

        if (user is null
            || user.RefreshTokenExpiresAt is null
            || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            return Result.Failure<TokenResponse>(InvalidRefreshToken);
        }

        var tokenResponse = jwtTokenService.GenerateToken(user.Id.ToString());
        var newHashedToken = jwtTokenService.HashToken(tokenResponse.RefreshToken);

        user.SetRefreshToken(newHashedToken, tokenResponse.RefreshTokenExpiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Refresh token rotated. UserId: {UserId}, Timestamp: {Timestamp}",
            user.Id,
            DateTime.UtcNow);

        return tokenResponse;
    }
}
