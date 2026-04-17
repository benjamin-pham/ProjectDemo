# Quickstart: CRUD API cho User và Role

**Tính năng**: `001-crud-user-role` | **Branch**: `001-crud-user-role`

Hướng dẫn nhanh để hiểu cách implement tính năng này, tập trung vào các điểm đặc thù so với codebase hiện có.

---

## Điểm khởi đầu

Tính năng này mở rộng trên nền tảng đã có:
- Auth feature (`src/ProjectTemplate.Application/Features/Auth/`) là template mẫu
- Pattern: `Command/Query → Handler → Endpoint` trong cùng một folder

---

## Các bước implement (tóm tắt thứ tự)

### Bước 1 — Refactor Domain (thực hiện trước, không tạo features mới)

1. Cập nhật `Role.cs`:
   - Đổi `ICollection<UserRole> UserRoles` → `ICollection<User> Users`
   - Thêm `static Role Create(name, description, type, permissions)` factory
   - Thêm `void Update(name, description, type, permissions)` method

2. Cập nhật `User.cs`:
   - Thêm `void AddRole(Role role)` (idempotent)
   - Thêm `void RemoveRole(Role role)`

3. Xóa các file liên quan đến explicit `UserRole` entity:
   ```
   src/ProjectTemplate.Domain/Entities/UserRole.cs
   src/ProjectTemplate.Domain/Repositories/IUserRoleRepository.cs
   src/ProjectTemplate.Infrastructure/Data/Configurations/UserRoleConfiguration.cs
   src/ProjectTemplate.Infrastructure/Repositories/UserRoleRepository.cs
   ```

4. Cập nhật `UserConfiguration.cs` — cấu hình implicit many-to-many giữ tên bảng `user_roles`

5. Cập nhật `AppDbContext.cs` — xóa `DbSet<UserRole>`

6. Cập nhật `RoleRepository.GetByUserIdAsync()` — không dùng `Context.UserRoles` nữa

7. Chạy `dotnet ef migrations add RefactorRoleImplicitManyToMany` và kiểm tra migration không thay đổi schema vật lý

### Bước 2 — Cập nhật Repository Interfaces

Thêm vào `IUserRepository`:
```csharp
Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
// (GetByIdAsync đã có — nhưng cần version load Roles: GetByIdWithRolesAsync)
Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default);
```

Thêm vào `IRoleRepository`:
```csharp
Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken ct = default);
Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
```

### Bước 3 — Features/Users (5 slices)

Mỗi slice nằm trong `src/ProjectTemplate.Application/Features/Users/{FeatureName}/`:

| Slice | Files |
|-------|-------|
| `GetUsers` | `GetUsersQuery.cs`, `GetUsersQueryHandler.cs` (Dapper), `UsersResponse.cs`, `GetUsersEndpoint.cs` |
| `GetUserById` | `GetUserByIdQuery.cs`, `GetUserByIdQueryHandler.cs` (Dapper), `UserDetailResponse.cs`, `GetUserByIdEndpoint.cs` |
| `CreateUser` | `CreateUserCommand.cs`, `CreateUserCommandHandler.cs`, `CreateUserCommandValidator.cs`, `CreateUserResponse.cs`, `CreateUserEndpoint.cs` |
| `UpdateUser` | `UpdateUserCommand.cs`, `UpdateUserCommandHandler.cs`, `UpdateUserCommandValidator.cs`, `UpdateUserEndpoint.cs` |
| `DeleteUser` | `DeleteUserCommand.cs`, `DeleteUserCommandHandler.cs`, `DeleteUserEndpoint.cs` |

### Bước 4 — Features/Users Role Assignment (2 slices)

| Slice | Files |
|-------|-------|
| `AssignRolesToUser` | `AssignRolesToUserCommand.cs`, `AssignRolesToUserCommandHandler.cs`, `AssignRolesToUserCommandValidator.cs`, `AssignRolesToUserEndpoint.cs` |
| `RemoveRoleFromUser` | `RemoveRoleFromUserCommand.cs`, `RemoveRoleFromUserCommandHandler.cs`, `RemoveRoleFromUserEndpoint.cs` |

### Bước 5 — Features/Roles (5 slices)

Tương tự Users, trong `src/ProjectTemplate.Application/Features/Roles/{FeatureName}/`.

### Bước 6 — Infrastructure (Implementation)

- Implement `UserRepository.GetByIdWithRolesAsync()`, `ExistsByUsernameAsync()`
- Implement `RoleRepository.ExistsByNameAsync()`, `ExistsByNameExcludingIdAsync()`, `GetByIdsAsync()`

---

## Pattern chuẩn: Query Handler (Dapper read)

```csharp
// GetUsersQueryHandler.cs
internal sealed class GetUsersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetUsersQuery, PagedResponse<UserListItemResponse>>
{
    public async Task<Result<PagedResponse<UserListItemResponse>>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                COUNT(*) OVER()     AS TotalCount,
                id                  AS Id,
                first_name          AS FirstName,
                last_name           AS LastName,
                username            AS Username,
                email               AS Email,
                phone               AS Phone,
                birthday            AS Birthday,
                created_at          AS CreatedAt
            FROM users
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var rows = await connection.QueryAsync<UserListItemResponse>(sql,
            new { request.PageSize, Offset = (request.Page - 1) * request.PageSize });

        var list = rows.ToList();
        var totalCount = list.Count > 0
            ? (int)connection.ExecuteScalar<long>("SELECT COUNT(*) FROM users WHERE is_deleted = false")
            : 0;
        // Hoặc dùng COUNT(*) OVER() từ kết quả đầu tiên

        return new PagedResponse<UserListItemResponse>(list, totalCount, request.Page, request.PageSize);
    }
}
```

---

## Pattern chuẩn: Command Handler (EF Core write)

```csharp
// CreateUserCommandHandler.cs
internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private static readonly Error UsernameAlreadyTaken =
        new("User.UsernameAlreadyTaken", "Username đã được sử dụng.");

    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
            return Result.Failure<CreateUserResponse>(UsernameAlreadyTaken);

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Username,
            passwordHash, request.Email, request.Phone, request.Birthday);

        userRepository.Add(user);

        return new CreateUserResponse(user.Id, user.Username, user.FirstName, user.LastName);
    }
}
```

---

## Pattern chuẩn: Endpoint

```csharp
// CreateUserEndpoint.cs
internal sealed class CreateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users", async (
            CreateUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value.Id}", result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Description,
                    statusCode: result.Error.Code == "User.UsernameAlreadyTaken"
                        ? StatusCodes.Status409Conflict
                        : StatusCodes.Status400BadRequest);
        })
        .RequireAuthorization()
        .WithName("CreateUser")
        .WithTags("Users")
        .Produces<CreateUserResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
    }
}
```

---

## Lưu ý quan trọng

1. **AssignRolesToUser handler** phải load User với `.Include(u => u.Roles)` — nếu không EF Core không thể theo dõi collection để thêm/xóa.
2. **DeleteRole handler** phải load Role với `.Include(r => r.Users)` để kiểm tra trước khi xóa.
3. **Dapper queries** phải dùng explicit column list — KHÔNG dùng `SELECT *`.
4. **Error codes** phải nhất quán với contracts (xem `contracts/`).
5. **Pagination** `COUNT(*) OVER()` trong Dapper cho phép lấy totalCount + items trong một query; dùng kỹ thuật này thay vì hai query riêng.
