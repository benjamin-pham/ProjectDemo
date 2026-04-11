using Dapper;
using MyProject.Application.Abstractions.Data;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Features.Roles.GetRoleById;

internal sealed class GetRoleByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetRoleByIdQuery, RoleDetailResponse>
{
    private static readonly Error RoleNotFound =
        new("Role.NotFound", "Vai trò không tồn tại.");

    public async Task<Result<RoleDetailResponse>> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                id           AS Id,
                name         AS Name,
                description  AS Description,
                type         AS Type,
                permissions  AS Permissions,
                created_at   AS CreatedAt,
                updated_at   AS UpdatedAt
            FROM roles
            WHERE id = @Id
              AND is_deleted = false
            """;

        var row = await connection.QuerySingleOrDefaultAsync<RoleRow>(sql, new { request.Id });

        if (row is null)
            return Result.Failure<RoleDetailResponse>(RoleNotFound);

        return new RoleDetailResponse(
            row.Id,
            row.Name,
            row.Description,
            row.Type,
            row.Permissions ?? [],
            row.CreatedAt,
            row.UpdatedAt);
    }

    private sealed record RoleRow(
        Guid Id,
        string Name,
        string Description,
        string Type,
        List<string>? Permissions,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
