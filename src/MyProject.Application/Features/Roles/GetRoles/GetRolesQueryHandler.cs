using Dapper;
using MyProject.Application.Abstractions.Data;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Features.Users.GetUsers;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Features.Roles.GetRoles;

internal sealed class GetRolesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetRolesQuery, PagedResponse<RoleListItemResponse>>
{
    public async Task<Result<PagedResponse<RoleListItemResponse>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                COUNT(*) OVER()  AS TotalCount,
                id               AS Id,
                name             AS Name,
                description      AS Description,
                type             AS Type,
                permissions      AS Permissions,
                created_at       AS CreatedAt
            FROM roles
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var rows = await connection.QueryAsync<RoleRow>(sql,
            new { request.PageSize, Offset = (request.Page - 1) * request.PageSize });

        var list = rows.ToList();
        var totalCount = list.Count > 0 ? list[0].TotalCount : 0;

        var items = list.Select(r => new RoleListItemResponse(
            r.Id, r.Name, r.Description, r.Type,
            r.Permissions ?? [], r.CreatedAt)).ToList();

        return new PagedResponse<RoleListItemResponse>(items, totalCount, request.Page, request.PageSize);
    }

    private sealed record RoleRow(
        int TotalCount,
        Guid Id,
        string Name,
        string Description,
        string Type,
        List<string>? Permissions,
        DateTime CreatedAt);
}
