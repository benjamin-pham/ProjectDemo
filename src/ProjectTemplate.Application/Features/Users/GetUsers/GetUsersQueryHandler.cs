using Dapper;
using ProjectTemplate.Application.Abstractions.Data;
using ProjectTemplate.Application.Abstractions.Messaging;
using ProjectTemplate.Domain.Abstractions;

namespace ProjectTemplate.Application.Features.Users.GetUsers;

internal sealed class GetUsersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetUsersQuery, PagedList<GetUsersResponse>>
{
    public async Task<Result<PagedList<GetUsersResponse>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id         AS Id,
                first_name AS FirstName,
                last_name  AS LastName,
                username   AS Username,
                email      AS Email,
                phone      AS Phone,
                birthday   AS Birthday,
                created_at AS CreatedAt
            FROM users
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset;

            SELECT COUNT(*)
            FROM users
            WHERE is_deleted = false;
            """;

        using var multi = await connection.QueryMultipleAsync(sql,
            new { request.PageSize, Offset = (request.PageNumber - 1) * request.PageSize });

        var items = (await multi.ReadAsync<GetUsersResponse>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedList<GetUsersResponse>(items, totalCount, request.PageNumber!.Value, request.PageSize!.Value);
    }
}
