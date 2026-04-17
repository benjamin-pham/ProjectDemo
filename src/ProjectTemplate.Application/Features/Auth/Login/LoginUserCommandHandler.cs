using ProjectTemplate.Application.Abstractions.Authentication;
using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;
using ProjectTemplate.Domain.Repositories;
using Microsoft.Extensions.Logging;
using ProjectTemplate.Application.Features.Auth.Shared;

namespace ProjectTemplate.Application.Features.Auth.Login;

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ILogger<LoginUserCommandHandler> logger)
    : ICommandHandler<LoginUserCommand, TokenResponse>
{

    public async Task<Result<TokenResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning(
                "Login failed. Username: {Username}, Reason: InvalidCredentials, Timestamp: {Timestamp}",
                request.Username,
                DateTime.UtcNow);

            return Result.Failure<TokenResponse>(UserErrors.InvalidCredentials);
        }

        var tokenResponse = jwtTokenService.GenerateToken(user.Id.ToString());
        var hashedToken = jwtTokenService.HashToken(tokenResponse.RefreshToken);

        user.SetRefreshToken(hashedToken, tokenResponse.RefreshTokenExpiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Login successful. UserId: {UserId}, Timestamp: {Timestamp}",
            user.Id,
            DateTime.UtcNow);

        return tokenResponse;
    }
}
