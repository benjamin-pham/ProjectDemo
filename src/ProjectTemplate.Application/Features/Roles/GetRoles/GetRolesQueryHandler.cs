using Dapper;
using ProjectTemplate.Application.Abstractions.Data;
using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Features.Roles.GetRoles;

internal sealed class GetRolesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetRolesQuery, PagedList<GetRolesResponse>>
{
    public async Task<Result<PagedList<GetRolesResponse>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        // sql begin
        const string sql = @"""
            SELECT
                id          AS Id,
                name        AS Name,
                description AS Description,
                type        AS Type,
                permissions AS Permissions,
                created_at  AS CreatedAt
            FROM roles
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset;

            SELECT COUNT(*)
            FROM roles
            WHERE is_deleted = false;
            """;
        // sql end
        using var multi = await connection.QueryMultipleAsync(sql, request);

        var items = (await multi.ReadAsync<GetRolesResponse>()).ToList();

        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedList<GetRolesResponse>(items, totalCount, request.PageNumber!.Value, request.PageSize!.Value);
    }
}
