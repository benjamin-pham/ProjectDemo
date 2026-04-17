# Data Model: CRUD API cho User và Role

**Tính năng**: `001-crud-user-role` | **Ngày**: 2026-04-11

---

## Thực thể hiện có và thay đổi cần thiết

### 1. `User` — `src/ProjectTemplate.Domain/Entities/User.cs`

**Trạng thái**: Tồn tại, cần bổ sung domain methods cho gán/gỡ Role.

| Thuộc tính | Kiểu | Nullable | Ràng buộc | Cột DB |
|------------|------|----------|-----------|--------|
| `Id` | `Guid` | No | PK, auto-gen | `id` |
| `FirstName` | `string` | No | Required, max 100 | `first_name` |
| `LastName` | `string` | No | Required, max 100 | `last_name` |
| `Username` | `string` | No | Required, unique (active), max 50 | `username` |
| `PasswordHash` | `string` | No | Required, max 512 | `password_hash` |
| `Email` | `string?` | Yes | Max 255 | `email` |
| `Phone` | `string?` | Yes | Max 20 | `phone` |
| `Birthday` | `DateOnly?` | Yes | — | `birthday` |
| `HashedRefreshToken` | `string?` | Yes | Max 512 | `hashed_refresh_token` |
| `RefreshTokenExpiresAt` | `DateTime?` | Yes | — | `refresh_token_expires_at` |
| `CreatedAt` | `DateTime` | No | Auto (BaseEntity) | `created_at` |
| `CreatedBy` | `string?` | Yes | Auto (BaseEntity) | `created_by` |
| `UpdatedAt` | `DateTime?` | Yes | Auto (BaseEntity) | `updated_at` |
| `UpdatedBy` | `string?` | Yes | Auto (BaseEntity) | `updated_by` |
| `IsDeleted` | `bool` | No | Soft delete flag | `is_deleted` |

**Navigation**:
- `ICollection<Role> Roles` — many-to-many implicit với `roles` qua bảng join `user_roles`

**Factory method** (hiện có):
```csharp
public static User Create(string firstName, string lastName, string username,
    string passwordHash, string? email, string? phone, DateOnly? birthday)
```

**Domain methods** (hiện có):
- `UpdateProfile(firstName, lastName, email, phone, birthday)` — cập nhật hồ sơ
- `SetRefreshToken(hashedToken, expiresAt)` — lưu refresh token
- `RevokeRefreshToken()` — thu hồi refresh token

**Domain methods cần thêm mới**:
```csharp
public void AddRole(Role role)
// Idempotent: bỏ qua nếu role đã được gán; không throw exception

public void RemoveRole(Role role)
// Nếu role không có trong danh sách: bỏ qua (no-op)
```

**Quy tắc validation** (trong Create/UpdateProfile):
- `firstName`, `lastName`: `ArgumentException.ThrowIfNullOrWhiteSpace`
- `username`: `ArgumentException.ThrowIfNullOrWhiteSpace`
- `passwordHash`: `ArgumentException.ThrowIfNullOrWhiteSpace`

---

### 2. `Role` — `src/ProjectTemplate.Domain/Entities/Role.cs`

**Trạng thái**: Tồn tại — cần bổ sung `Create()` factory, đổi `UserRoles` → `Users` navigation, thêm `Update()` method.

| Thuộc tính | Kiểu | Nullable | Ràng buộc | Cột DB |
|------------|------|----------|-----------|--------|
| `Id` | `Guid` | No | PK, auto-gen | `id` |
| `Name` | `string` | No | Required, unique, max 100 | `name` |
| `Description` | `string` | No | Required, max 500 | `description` |
| `Type` | `RoleType` | No | Enum stored as string | `type` |
| `Permissions` | `List<string>` | No | Array tự do, PostgreSQL `text[]` | `permissions` |
| `CreatedAt` | `DateTime` | No | Auto (BaseEntity) | `created_at` |
| `CreatedBy` | `string?` | Yes | Auto (BaseEntity) | `created_by` |
| `UpdatedAt` | `DateTime?` | Yes | Auto (BaseEntity) | `updated_at` |
| `UpdatedBy` | `string?` | Yes | Auto (BaseEntity) | `updated_by` |
| `IsDeleted` | `bool` | No | Soft delete flag | `is_deleted` |

**Navigation** (sau refactor):
- `ICollection<User> Users` *(đổi tên từ `UserRoles` → `Users`)* — many-to-many implicit với `users` qua `user_roles`

**Factory method cần thêm** (xác nhận trong clarification):
```csharp
public static Role Create(string name, string description, RoleType type, List<string> permissions)
// ArgumentException.ThrowIfNullOrWhiteSpace(name)
```

**Domain method cần thêm**:
```csharp
public void Update(string name, string description, RoleType type, List<string> permissions)
// ArgumentException.ThrowIfNullOrWhiteSpace(name)
```

---

### 3. `UserRole` — `src/ProjectTemplate.Domain/Entities/UserRole.cs`

**Trạng thái**: **Xóa** — thay thế bởi EF Core implicit many-to-many.

Sau refactor, EF Core tự quản lý bảng `user_roles` qua implicit mapping. Các file cần xóa:
- `src/ProjectTemplate.Domain/Entities/UserRole.cs`
- `src/ProjectTemplate.Domain/Repositories/IUserRoleRepository.cs`
- `src/ProjectTemplate.Infrastructure/Data/Configurations/UserRoleConfiguration.cs`
- `src/ProjectTemplate.Infrastructure/Repositories/UserRoleRepository.cs`

---

### 4. `RoleType` — `src/ProjectTemplate.Domain/Enumerations/RoleType.cs`

**Trạng thái**: Không thay đổi.

```csharp
public enum RoleType
{
    System = 1,
    Dynamic = 2
}
```

---

## Schema DB (bảng và quan hệ)

```
users                           roles
──────────────────────────      ──────────────────────────
id (uuid, PK)                   id (uuid, PK)
first_name (varchar 100)        name (varchar 100, unique)
last_name (varchar 100)         description (varchar 500)
username (varchar 50, unique)   type (varchar 50)
password_hash (varchar 512)     permissions (text[])
email (varchar 255, nullable)   created_at (timestamptz)
phone (varchar 20, nullable)    created_by (text, nullable)
birthday (date, nullable)       updated_at (timestamptz, nullable)
hashed_refresh_token (nullable) updated_by (text, nullable)
refresh_token_expires_at (null) is_deleted (bool)
created_at (timestamptz)
created_by (text, nullable)               user_roles (implicit join)
updated_at (timestamptz, nullable)        ─────────────────────────
updated_by (text, nullable)               user_id (uuid, FK → users.id)
is_deleted (bool)                         role_id (uuid, FK → roles.id)
                                          PK (user_id, role_id)
```

**Indexes**:
- `ix_users_username` — UNIQUE WHERE `is_deleted = false`
- `ix_roles_name` — UNIQUE
- `fk_user_roles_user_id` — CASCADE DELETE khi xóa User
- `fk_user_roles_role_id` — CASCADE DELETE khi xóa Role (nhưng DeleteRole handler sẽ check trước)

---

## EF Configuration thay đổi

### `UserConfiguration.cs` — thay đổi phần many-to-many

```csharp
// Thay thế:
builder.HasMany(u => u.Roles)
    .WithMany()
    .UsingEntity<UserRole>();

// Bằng:
builder.HasMany(u => u.Roles)
    .WithMany(r => r.Users)
    .UsingEntity("user_roles",
        l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("role_id")
              .HasConstraintName("fk_user_roles_role_id").OnDelete(DeleteBehavior.Cascade),
        r => r.HasOne(typeof(User)).WithMany().HasForeignKey("user_id")
              .HasConstraintName("fk_user_roles_user_id").OnDelete(DeleteBehavior.Cascade),
        j => {
            j.ToTable("user_roles");
            j.HasKey("user_id", "role_id");
        });
```

### `AppDbContext.cs` — xóa `DbSet<UserRole>`

```csharp
// Xóa dòng:
public DbSet<UserRole> UserRoles => Set<UserRole>();

// Giữ lại (hoặc đổi thành property đầy đủ):
public DbSet<User> Users { get; init; }
public DbSet<Role> Roles { get; init; }
```

---

## State Transitions

### User lifecycle

```
[Không tồn tại] → Create() → [Active]
[Active] → UpdateProfile() → [Active, thông tin đã cập nhật]
[Active] → AddRole(role) → [Active, có thêm role]
[Active] → RemoveRole(role) → [Active, không còn role đó]
[Active] → DELETE endpoint → [Xóa khỏi DB]
```

### Role lifecycle

```
[Không tồn tại] → Create() → [Active]
[Active] → Update() → [Active, thông tin đã cập nhật]
[Active, không có User] → DELETE endpoint → [Xóa khỏi DB]
[Active, có User] → DELETE endpoint → [Lỗi 409, không xóa]
```
