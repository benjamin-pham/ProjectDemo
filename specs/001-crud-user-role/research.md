# Research: CRUD API cho User và Role

**Tính năng**: `001-crud-user-role` | **Ngày**: 2026-04-11

Tài liệu này ghi lại các quyết định kỹ thuật được rút ra trong quá trình phân tích codebase hiện tại và đặc tả tính năng. Không có NEEDS CLARIFICATION nào còn mở sau khi đọc spec, clarifications, và source code.

---

## Q1: Many-to-many User ↔ Role: tiếp tục dùng explicit `UserRole` entity hay chuyển sang implicit?

**Quyết định**: Chuyển hoàn toàn sang **EF Core implicit many-to-many**.

**Lý do**: Spec clarification 2026-04-11 đã xác nhận: "Navigation trực tiếp cả hai chiều (`User.Roles` + `Role.Users`), EF Core implicit many-to-many, không cần `UserRole` entity." `Role.UserRoles` phải được đổi thành `Role.Users`. Giữ explicit `UserRole` sẽ mâu thuẫn với nav property mới.

**Cách thực hiện**:
- Xóa `UserRole.cs`, `UserRoleConfiguration.cs`, `IUserRoleRepository.cs`, `UserRoleRepository.cs`
- Đổi `Role.UserRoles` → `Role.Users` (`ICollection<User>`)
- Cập nhật `UserConfiguration` thành `HasMany(u => u.Roles).WithMany(r => r.Users).UsingEntity("user_roles")` để giữ tên bảng `user_roles` hiện có
- Xóa `DbSet<UserRole>` khỏi `AppDbContext`
- Tạo EF migration mới; bảng vật lý không thay đổi — chỉ EF model snapshot thay đổi
- Cập nhật `RoleRepository.GetByUserIdAsync()` dùng `r.Users.Any(u => u.Id == userId)` thay vì `Context.UserRoles.Any(...)`

**Lựa chọn đã cân nhắc**:
- Giữ explicit `UserRole` + thêm `Role.Users` — không khả thi: EF không cho phép cả hai kiểu nav trên cùng một join
- Chỉ đổi tên `UserRoles → Users` mà giữ entity — vi phạm spec clarification

---

## Q2: Thao tác gán/gỡ Role cho User: xử lý qua handler hay domain method?

**Quyết định**: Xử lý qua **domain method trên `User` entity** — `AddRole(role)` và `RemoveRole(role)`.

**Lý do**: Constitution II (Rich Domain Model) yêu cầu mutation qua named domain methods. Handler chỉ load entity, gọi method, rồi persist qua EF Core (implicit many-to-many sẽ tự quản lý join table).

**Cách thực hiện**:
```csharp
// User.cs
public void AddRole(Role role)
{
    if (!Roles.Any(r => r.Id == role.Id))
        Roles.Add(role);
    // Idempotent: bỏ qua nếu đã tồn tại
}

public void RemoveRole(Role role)
{
    var existing = Roles.FirstOrDefault(r => r.Id == role.Id);
    if (existing is not null)
        Roles.Remove(existing);
}
```
Handler cần load `User` với `.Include(u => u.Roles)` trước khi gọi domain method.

**Lựa chọn đã cân nhắc**:
- Gán trực tiếp từ handler (`user.Roles.Add(role)`) — vi phạm Constitution II
- Dùng `IUserRoleRepository.AddAsync()` — không phù hợp sau khi đã xóa explicit entity

---

## Q3: Xóa Role đang được gán cho User: cho phép hay từ chối?

**Quyết định**: **Từ chối** — trả về lỗi `Role.HasActiveAssignments` (HTTP 409 Conflict).

**Lý do**: Spec Assumptions ghi rõ: "Khi xóa Role đang được gán cho User, hệ thống sẽ từ chối thao tác và trả về lỗi phù hợp để bảo toàn tính toàn vẹn dữ liệu." Đây là hành vi explicit, không phải cascade delete.

**Cách thực hiện**:
- Handler `DeleteRoleCommandHandler` kiểm tra `role.Users.Any()` sau khi load với `.Include(r => r.Users)`
- Nếu có user đang gán → `Result.Failure(Role.HasActiveAssignments)`
- Endpoint trả về HTTP 409 Conflict

---

## Q4: Pagination cho danh sách User và Role

**Quyết định**: Dùng **Dapper + OFFSET/LIMIT** với query parameters `page` (default: 1) và `pageSize` (default: 20, max: 100).

**Lý do**: Constitution Technology Stack chỉ định Dapper cho read side (CQRS). Pagination đơn giản bằng SQL `OFFSET/LIMIT` đủ tốt với 10.000 bản ghi; không cần keyset pagination ở giai đoạn này.

**Response shape**:
```json
{
  "items": [...],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

**Cách thực hiện**:
- Query: `COUNT(*) OVER()` để lấy `totalCount` cùng lúc với `items` trong một query
- Validation: `pageSize` ≤ 100 bởi FluentValidation trong query validator

---

## Q5: URL convention cho User/Role endpoints

**Quyết định**: Theo RESTful chuẩn, prefix `/api/`:

| Method | URL | Chức năng |
|--------|-----|-----------|
| GET | `/api/users` | Danh sách User (có phân trang) |
| GET | `/api/users/{id}` | Chi tiết User |
| POST | `/api/users` | Tạo User mới |
| PUT | `/api/users/{id}` | Cập nhật User |
| DELETE | `/api/users/{id}` | Xóa User |
| POST | `/api/users/{id}/roles` | Gán Role cho User |
| DELETE | `/api/users/{id}/roles/{roleId}` | Gỡ Role khỏi User |
| GET | `/api/roles` | Danh sách Role |
| GET | `/api/roles/{id}` | Chi tiết Role |
| POST | `/api/roles` | Tạo Role mới |
| PUT | `/api/roles/{id}` | Cập nhật Role |
| DELETE | `/api/roles/{id}` | Xóa Role |

**Lý do**: Tuân theo pattern hiện có (`/api/auth/...`) trong project. Nested resource `/api/users/{id}/roles` là chuẩn REST cho quan hệ nhiều-nhiều.

---

## Q6: Xác thực (authentication) cho các endpoints mới

**Quyết định**: Tất cả endpoints dùng `.RequireAuthorization()` — tái sử dụng cơ chế JWT hiện có.

**Lý do**: Spec assumption ghi: "Hệ thống xác thực và phân quyền hiện tại sẽ được tái sử dụng". Cơ chế JWT Bearer đã được cấu hình trong `AuthenticationExtensions.cs`. Chưa triển khai role-based authorization (`RequireAuthorization("Admin")`) trong phiên bản này — FR-016 chỉ yêu cầu xác thực, không phân quyền chi tiết.

---

## Q7: Response DTO: trả về password hash hay không?

**Quyết định**: **Không bao giờ** trả về `PasswordHash` trong response. Dapper query chỉ SELECT các cột cần thiết.

**Lý do**: FR-004 bắt buộc. Queries viết bằng Dapper với explicit column list — không dùng `SELECT *` — đảm bảo password hash không bao giờ lọt vào response.

---

## Q8: EF migration strategy cho refactor many-to-many

**Quyết định**: Tạo một EF migration mới tên `RefactorRoleImplicitManyToMany`. Migration này về mặt vật lý **không thay đổi schema** (bảng `user_roles` giữ nguyên), chỉ cập nhật EF model snapshot để phản ánh implicit many-to-many thay vì explicit `UserRole` entity.

**Lý do**: EF Core đã tạo bảng `user_roles` với `user_id` và `role_id` trong migration `AddUsersTable`. Implicit many-to-many sẽ map vào cùng bảng đó qua `UsingEntity("user_roles")`. Cần kiểm tra migration generated để đảm bảo không có thay đổi schema ngoài ý muốn (ví dụ: drop/recreate index).
