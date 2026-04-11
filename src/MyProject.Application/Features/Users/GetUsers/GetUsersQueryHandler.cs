using Dapper;
using MyProject.Application.Abstractions.Data;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Features.Users.GetUsers;

internal sealed class GetUsersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetUsersQuery, PagedResponse<UserListItemResponse>>
{
    public async Task<Result<PagedResponse<UserListItemResponse>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                COUNT(*) OVER()  AS TotalCount,
                id               AS Id,
                first_name       AS FirstName,
                last_name        AS LastName,
                username         AS Username,
                email            AS Email,
                phone            AS Phone,
                birthday         AS Birthday,
                created_at       AS CreatedAt
            FROM users
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var rows = await connection.QueryAsync<UserRow>(sql,
            new { request.PageSize, Offset = (request.Page - 1) * request.PageSize });

        var list = rows.ToList();
        var totalCount = list.Count > 0 ? list[0].TotalCount : 0;

        var items = list.Select(r => new UserListItemResponse(
            r.Id, r.FirstName, r.LastName, r.Username,
            r.Email, r.Phone, r.Birthday, r.CreatedAt)).ToList();

        return new PagedResponse<UserListItemResponse>(items, totalCount, request.Page, request.PageSize);
    }

    private sealed record UserRow(
        int TotalCount,
        Guid Id,
        string FirstName,
        string LastName,
        string Username,
        string? Email,
        string? Phone,
        DateOnly? Birthday,
        DateTime CreatedAt);
}
