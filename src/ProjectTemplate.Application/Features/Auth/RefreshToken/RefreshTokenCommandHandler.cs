using ProjectTemplate.Application.Abstractions.Authentication;
using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;
using Microsoft.Extensions.Logging;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Features.Auth.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger)
    : ICommandHandler<RefreshTokenCommand, TokenResponse>
{

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
            return Result.Failure<TokenResponse>(UserErrors.InvalidRefreshToken);
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
