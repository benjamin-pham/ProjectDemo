using Dapper;
using MyProject.Application.Abstractions.Data;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Errors;

namespace MyProject.Application.Features.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetUserByIdQuery, UserDetailResponse>
{

    public async Task<Result<UserDetailResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                u.id            AS UserId,
                u.first_name    AS FirstName,
                u.last_name     AS LastName,
                u.username      AS Username,
                u.email         AS Email,
                u.phone         AS Phone,
                u.birthday      AS Birthday,
                u.created_at    AS CreatedAt,
                r.id            AS RoleId,
                r.name          AS RoleName,
                r.type          AS RoleType
            FROM users u
            LEFT JOIN user_roles ur ON ur.user_id = u.id
            LEFT JOIN roles r ON r.id = ur.role_id AND r.is_deleted = false
            WHERE u.id = @Id
              AND u.is_deleted = false
            """;

        var rows = await connection.QueryAsync<UserRow>(sql, new { request.Id });
        var list = rows.ToList();

        if (list.Count == 0)
            return Result.Failure<UserDetailResponse>(UserErrors.NotFound);

        var first = list[0];
        var roles = list
            .Where(r => r.RoleId.HasValue)
            .Select(r => new UserRoleItem(r.RoleId!.Value, r.RoleName!, r.RoleType!))
            .ToList();

        return new UserDetailResponse(
            first.UserId,
            first.FirstName,
            first.LastName,
            first.Username,
            first.Email,
            first.Phone,
            first.Birthday,
            first.CreatedAt,
            roles);
    }

    private sealed record UserRow(
        Guid UserId,
        string FirstName,
        string LastName,
        string Username,
        string? Email,
        string? Phone,
        DateOnly? Birthday,
        DateTime CreatedAt,
        Guid? RoleId,
        string? RoleName,
        string? RoleType);
}
