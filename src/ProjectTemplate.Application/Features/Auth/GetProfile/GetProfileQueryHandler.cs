using Dapper;
using ProjectTemplate.Application.Abstractions.Data;
using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Domain.Errors;

namespace ProjectTemplate.Application.Features.Auth.GetProfile;

internal sealed class GetProfileQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IUserContext userContext)
    : IQueryHandler<GetProfileQuery, UserProfileResponse>
{

    public async Task<Result<UserProfileResponse>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id          AS UserId,
                first_name  AS FirstName,
                last_name   AS LastName,
                username    AS Username,
                email       AS Email,
                phone       AS Phone,
                birthday    AS Birthday,
                created_at  AS CreatedAt
            FROM users
            WHERE id = @UserId
              AND is_deleted = false
            """;

        var profile = await connection.QuerySingleOrDefaultAsync<UserProfileResponse>(
            sql,
            new { UserId = userContext.UserId });

        return profile is not null
            ? profile
            : Result.Failure<UserProfileResponse>(UserErrors.NotFound);
    }
}
