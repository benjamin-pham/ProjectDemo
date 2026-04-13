using Dapper;
using MyProject.Application.Abstractions.Data;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Features.Roles.GetRoles;

internal sealed class GetRolesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetRolesQuery, PagedList<GetRolesResponse>>
{
    public async Task<Result<PagedList<GetRolesResponse>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
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

        using var multi = await connection.QueryMultipleAsync(sql,
            new { request.PageSize, Offset = (request.PageNumber - 1) * request.PageSize });

        var items = (await multi.ReadAsync<GetRolesResponse>()).ToList();

        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedList<GetRolesResponse>(items, totalCount, request.PageNumber!.Value, request.PageSize!.Value);
    }
}
