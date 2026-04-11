# Danh sách tác vụ: CRUD API cho User và Role

<!-- Language: Vietnamese — all prose, task descriptions, and headings in Vietnamese.
     Code, file paths, identifiers, and code comments remain in English. -->

**Đầu vào**: Tài liệu thiết kế từ `/specs/001-crud-user-role/`
**Yêu cầu tiên quyết**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Không yêu cầu — spec không đề nghị TDD. Test tasks không được tạo ra trong phiên bản này.

**Tổ chức**: Tasks được nhóm theo user story để mỗi story có thể được implement và kiểm thử độc lập.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Có thể chạy song song với tasks khác trong cùng phase (khác file, không có dependency chưa hoàn thành)
- **[Story]**: User story mà task này thuộc về (US1, US2, US3)
- Mỗi task bao gồm đường dẫn file cụ thể

---

## Phase 1: Setup (Kiểm tra trạng thái ban đầu)

**Mục đích**: Xác nhận codebase hiện tại sạch trước khi refactor.

- [X] T001 Xác nhận project build thành công trước khi bắt đầu thay đổi: chạy `dotnet build src/MyProject.sln` và đảm bảo 0 lỗi

---

## Phase 2: Foundational (Domain Refactor — Blocking Prerequisites)

**Mục đích**: Refactor implicit many-to-many, bổ sung domain methods, cập nhật EF config và repository interfaces. **Phải hoàn thành toàn bộ phase này trước khi bắt đầu bất kỳ User Story nào.**

**⚠️ CRITICAL**: Không có user story nào có thể bắt đầu cho đến khi phase này hoàn tất.

### 2.1 — Domain Layer

- [X] T002 [P] Cập nhật `User.cs`: thêm `AddRole(Role role)` (idempotent — bỏ qua nếu role đã gán) và `RemoveRole(Role role)` (no-op nếu chưa gán) vào `src/MyProject.Domain/Entities/User.cs`
- [X] T003 [P] Cập nhật `Role.cs`: thêm `static Role Create(string name, string description, RoleType type, List<string> permissions)` factory với `ArgumentException.ThrowIfNullOrWhiteSpace(name)`, thêm `void Update(string name, string description, RoleType type, List<string> permissions)` method, đổi tên nav property `UserRoles` → `Users` (`ICollection<User>`) trong `src/MyProject.Domain/Entities/Role.cs`
- [X] T004 [P] Xóa file `src/MyProject.Domain/Entities/UserRole.cs` (explicit join entity không còn dùng sau khi chuyển sang implicit many-to-many)
- [X] T005 [P] Xóa file `src/MyProject.Domain/Repositories/IUserRoleRepository.cs`
- [X] T006 [P] Extend `IUserRepository`: thêm `Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)` và `Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct = default)` vào `src/MyProject.Domain/Repositories/IUserRepository.cs`
- [X] T007 [P] Extend `IRoleRepository`: thêm `Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)`, `Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken ct = default)`, `Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)` vào `src/MyProject.Domain/Repositories/IRoleRepository.cs`

### 2.2 — Infrastructure Layer

- [X] T008 [P] Xóa file `src/MyProject.Infrastructure/Data/Configurations/UserRoleConfiguration.cs`
- [X] T009 [P] Xóa file `src/MyProject.Infrastructure/Repositories/UserRoleRepository.cs`
- [X] T010 Cập nhật `UserConfiguration.cs`: thay thế `HasMany(u => u.Roles).WithMany().UsingEntity<UserRole>()` bằng `HasMany(u => u.Roles).WithMany(r => r.Users).UsingEntity("user_roles", ...)` giữ tên bảng `user_roles`, `user_id`/`role_id` FK, cascade delete — xem data-model.md cho EF config đầy đủ trong `src/MyProject.Infrastructure/Data/Configurations/UserConfiguration.cs`
- [X] T011 Cập nhật `AppDbContext.cs`: xóa `DbSet<UserRole>`, đảm bảo `DbSet<Role> Roles` tồn tại, xóa mọi tham chiếu đến `UserRole` trong `src/MyProject.Infrastructure/Data/AppDbContext.cs`
- [X] T012 Cập nhật `RoleRepository.GetByUserIdAsync()`: thay `Context.UserRoles.Any(ur => ur.RoleId == roleId && ur.UserId == userId)` bằng `r.Users.Any(u => u.Id == userId)` — dùng EF query qua nav property mới trong `src/MyProject.Infrastructure/Repositories/RoleRepository.cs`
- [X] T013 Implement `UserRepository` new methods: `ExistsByUsernameAsync` (EF query `!is_deleted && username == x`) và `GetByIdWithRolesAsync` (EF `.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == id && !u.IsDeleted)`) trong `src/MyProject.Infrastructure/Repositories/UserRepository.cs`
- [X] T014 Implement `RoleRepository` new methods: `ExistsByNameAsync`, `ExistsByNameExcludingIdAsync`, `GetByIdsAsync` (batch load bằng `.Where(r => ids.Contains(r.Id) && !r.IsDeleted).ToListAsync()`) trong `src/MyProject.Infrastructure/Repositories/RoleRepository.cs`
- [X] T015 Tạo EF migration: chạy `dotnet ef migrations add RefactorRoleImplicitManyToMany --project src/MyProject.Infrastructure --startup-project src/MyProject.API`; kiểm tra file migration được tạo — đảm bảo **không có thay đổi schema vật lý** (bảng `user_roles` giữ nguyên), chỉ cập nhật model snapshot trong `src/MyProject.Infrastructure/Data/Migrations/`

**Checkpoint**: Foundational hoàn tất — project phải build thành công (0 errors). Bắt đầu các User Story.

---

## Phase 3: User Story 1 — Quản lý người dùng (Priority: P1) 🎯 MVP

**Goal**: Cung cấp đầy đủ 5 endpoints CRUD cho User: GET list, GET by id, POST, PUT, DELETE. Tất cả yêu cầu JWT auth.

**Independent Test**: Gọi lần lượt 5 endpoints của User với JWT hợp lệ và xác nhận: danh sách trả về có phân trang, GET by id trả về roles, POST tạo user mới (no password in response), PUT cập nhật profile, DELETE xóa khỏi DB.

### Slice: GetUsers

- [X] T016 [P] [US1] Implement `GetUsers` slice: `GetUsersQuery.cs` (record với `Page`, `PageSize` int), `GetUsersQueryHandler.cs` (Dapper: `COUNT(*) OVER()` + `LIMIT @PageSize OFFSET @Offset`, WHERE `is_deleted = false`, SELECT không bao gồm `password_hash`/`hashed_refresh_token`), `UsersResponse.cs` (record PagedResponse với items `UserListItemResponse`) trong `src/MyProject.Application/Features/Users/GetUsers/`
- [X] T017 [P] [US1] Implement `GetUsersEndpoint.cs`: `GET /api/users`, query params `page`/`pageSize`, `.RequireAuthorization()`, `.WithTags("Users")`, trả về `200 OK PagedResponse<UserListItemResponse>` trong `src/MyProject.Application/Features/Users/GetUsers/GetUsersEndpoint.cs`

### Slice: GetUserById

- [X] T018 [P] [US1] Implement `GetUserById` slice: `GetUserByIdQuery.cs` (record với `Guid Id`), `GetUserByIdQueryHandler.cs` (Dapper: JOIN `user_roles` → `roles` để lấy danh sách roles, trả về 404 `User.NotFound` nếu không tìm thấy hoặc `is_deleted = true`), `UserDetailResponse.cs` (record bao gồm `IReadOnlyList<UserRoleItem> Roles`) trong `src/MyProject.Application/Features/Users/GetUserById/`
- [X] T019 [P] [US1] Implement `GetUserByIdEndpoint.cs`: `GET /api/users/{id}`, trả về `200 OK UserDetailResponse`, `404 Problem` với title `User.NotFound`, `400 Problem` khi id không hợp lệ trong `src/MyProject.Application/Features/Users/GetUserById/GetUserByIdEndpoint.cs`

### Slice: CreateUser

- [X] T020 [P] [US1] Implement `CreateUser` slice: `CreateUserCommand.cs` (record với FirstName, LastName, Username, Password, Email?, Phone?, Birthday?), `CreateUserCommandValidator.cs` (FluentValidation: NotEmpty/MaxLength/email format/password min 8 chars uppercase+lowercase+digit), `CreateUserCommandHandler.cs` (gọi `ExistsByUsernameAsync` → 409 `User.UsernameAlreadyTaken` nếu trùng, gọi `IPasswordHasher.Hash()`, gọi `User.Create()`, `userRepository.Add()`, return `CreateUserResponse`), `CreateUserResponse.cs` trong `src/MyProject.Application/Features/Users/CreateUser/`
- [X] T021 [P] [US1] Implement `CreateUserEndpoint.cs`: `POST /api/users`, trả về `201 Created` với `Location: /api/users/{id}`, body `CreateUserResponse` (không có password), `400` khi validation fail, `409` khi username trùng trong `src/MyProject.Application/Features/Users/CreateUser/CreateUserEndpoint.cs`

### Slice: UpdateUser

- [X] T022 [P] [US1] Implement `UpdateUser` slice: `UpdateUserCommand.cs` (record với Guid Id, FirstName, LastName, Email?, Phone?, Birthday?), `UpdateUserCommandValidator.cs` (NotEmpty/MaxLength cho firstName/lastName), `UpdateUserCommandHandler.cs` (load user bằng `GetByIdAsync`, 404 nếu không tìm thấy, gọi `user.UpdateProfile(...)`, return updated response) trong `src/MyProject.Application/Features/Users/UpdateUser/`
- [X] T023 [P] [US1] Implement `UpdateUserEndpoint.cs`: `PUT /api/users/{id}` — bind path `id` + body, `.RequireAuthorization()`, trả về `200 OK` với profile đã cập nhật, `404 Problem` nếu không tìm thấy trong `src/MyProject.Application/Features/Users/UpdateUser/UpdateUserEndpoint.cs`

### Slice: DeleteUser

- [X] T024 [P] [US1] Implement `DeleteUser` slice: `DeleteUserCommand.cs` (record với Guid Id), `DeleteUserCommandHandler.cs` (load user, 404 nếu không tìm thấy, `userRepository.Delete(user)` — hard delete) trong `src/MyProject.Application/Features/Users/DeleteUser/`
- [X] T025 [P] [US1] Implement `DeleteUserEndpoint.cs`: `DELETE /api/users/{id}`, trả về `204 No Content` khi xóa thành công, `404 Problem` nếu không tìm thấy trong `src/MyProject.Application/Features/Users/DeleteUser/DeleteUserEndpoint.cs`

**Checkpoint**: Tại đây, US1 phải hoạt động hoàn toàn độc lập. Test 5 endpoints với JWT hợp lệ.

---

## Phase 4: User Story 2 — Quản lý vai trò (Priority: P2)

**Goal**: Cung cấp đầy đủ 5 endpoints CRUD cho Role: GET list, GET by id, POST, PUT, DELETE với business rule: không xóa Role đang được gán.

**Independent Test**: Gọi 5 endpoints Role — xác nhận GET list có phân trang + permissions, POST tạo role mới với name unique, PUT cập nhật thành công, DELETE trả về 409 nếu role đang gán, 204 nếu không ai dùng.

### Slice: GetRoles

- [X] T026 [P] [US2] Implement `GetRoles` slice: `GetRolesQuery.cs` (Page, PageSize), `GetRolesQueryHandler.cs` (Dapper: `COUNT(*) OVER()`, SELECT id/name/description/type/permissions/created_at từ `roles` WHERE `is_deleted = false`), `RolesResponse.cs` (PagedResponse<RoleListItemResponse> với field `permissions text[]`) trong `src/MyProject.Application/Features/Roles/GetRoles/`
- [ ] T027 [P] [US2] Implement `GetRolesEndpoint.cs`: `GET /api/roles`, query params `page`/`pageSize`, trả về `200 OK PagedResponse<RoleListItemResponse>` trong `src/MyProject.Application/Features/Roles/GetRoles/GetRolesEndpoint.cs`

### Slice: GetRoleById

- [ ] T028 [P] [US2] Implement `GetRoleById` slice: `GetRoleByIdQuery.cs` (Guid Id), `GetRoleByIdQueryHandler.cs` (Dapper: SELECT đầy đủ từ `roles`, 404 `Role.NotFound` nếu không tìm thấy), `RoleDetailResponse.cs` (bao gồm `updatedAt`) trong `src/MyProject.Application/Features/Roles/GetRoleById/`
- [ ] T029 [P] [US2] Implement `GetRoleByIdEndpoint.cs`: `GET /api/roles/{id}`, `200 OK RoleDetailResponse`, `404 Problem` với title `Role.NotFound` trong `src/MyProject.Application/Features/Roles/GetRoleById/GetRoleByIdEndpoint.cs`

### Slice: CreateRole

- [ ] T030 [P] [US2] Implement `CreateRole` slice: `CreateRoleCommand.cs` (Name, Description, Type string, Permissions List<string>), `CreateRoleCommandValidator.cs` (NotEmpty/MaxLength cho name/description, Must be "System" hoặc "Dynamic" cho type, NotNull cho permissions), `CreateRoleCommandHandler.cs` (gọi `ExistsByNameAsync` → 409 `Role.NameAlreadyTaken`, gọi `Role.Create(name, description, roleType, permissions)`, `roleRepository.Add(role)`, return response), `CreateRoleResponse.cs` trong `src/MyProject.Application/Features/Roles/CreateRole/`
- [ ] T031 [P] [US2] Implement `CreateRoleEndpoint.cs`: `POST /api/roles`, `201 Created` với `Location: /api/roles/{id}`, body `CreateRoleResponse`, `400` validation, `409` name trùng trong `src/MyProject.Application/Features/Roles/CreateRole/CreateRoleEndpoint.cs`

### Slice: UpdateRole

- [ ] T032 [P] [US2] Implement `UpdateRole` slice: `UpdateRoleCommand.cs` (Guid Id, Name, Description, Type, Permissions), `UpdateRoleCommandValidator.cs` (cùng rules với Create), `UpdateRoleCommandHandler.cs` (load role, 404 nếu không tìm thấy, gọi `ExistsByNameExcludingIdAsync` → 409 nếu tên trùng role khác, gọi `role.Update(...)`, return updated response) trong `src/MyProject.Application/Features/Roles/UpdateRole/`
- [ ] T033 [P] [US2] Implement `UpdateRoleEndpoint.cs`: `PUT /api/roles/{id}`, `200 OK` với role đã cập nhật, `404 Problem`, `409 Problem` khi tên trùng trong `src/MyProject.Application/Features/Roles/UpdateRole/UpdateRoleEndpoint.cs`

### Slice: DeleteRole

- [ ] T034 [P] [US2] Implement `DeleteRole` slice: `DeleteRoleCommand.cs` (Guid Id), `DeleteRoleCommandHandler.cs` (load role với `.Include(r => r.Users)`, 404 nếu không tìm thấy, kiểm tra `role.Users.Any()` → 409 `Role.HasActiveAssignments` nếu có user đang gán, `roleRepository.Delete(role)` — hard delete) trong `src/MyProject.Application/Features/Roles/DeleteRole/`
- [ ] T035 [P] [US2] Implement `DeleteRoleEndpoint.cs`: `DELETE /api/roles/{id}`, `204 No Content` thành công, `404 Problem`, `409 Problem` với title `Role.HasActiveAssignments` trong `src/MyProject.Application/Features/Roles/DeleteRole/DeleteRoleEndpoint.cs`

**Checkpoint**: US1 và US2 phải hoạt động độc lập. Test 10 endpoints User + Role đầy đủ.

---

## Phase 5: User Story 3 — Gán và gỡ bỏ vai trò (Priority: P3)

**Goal**: Cung cấp 2 endpoints quản lý gán/gỡ Role cho User: POST assign (idempotent), DELETE remove.

**Independent Test**: Gán role cho user → xác nhận GET /api/users/{id} trả về role đó. Gỡ role → xác nhận role không còn trong list. Gán lại role đã có → 200 OK không lỗi.

### Slice: AssignRolesToUser

- [ ] T036 [P] [US3] Implement `AssignRolesToUser` slice: `AssignRolesToUserCommand.cs` (Guid UserId, List<Guid> RoleIds), `AssignRolesToUserCommandValidator.cs` (NotEmpty cho RoleIds, mỗi id phải là Guid không rỗng), `AssignRolesToUserCommandHandler.cs` (load User với `.Include(u => u.Roles)` via `GetByIdWithRolesAsync` → 404 `User.NotFound`, batch load roles via `GetByIdsAsync` → 404 `Role.NotFound` nếu có id không tồn tại, gọi `user.AddRole(role)` cho từng role — idempotent, return response với toàn bộ roles hiện tại của user) trong `src/MyProject.Application/Features/Users/AssignRolesToUser/`
- [ ] T037 [P] [US3] Implement `AssignRolesToUserEndpoint.cs`: `POST /api/users/{id}/roles`, body `{ roleIds: [...] }`, `200 OK` với `{ userId, roles: [...] }`, `404 Problem` (User.NotFound hoặc Role.NotFound), `400 Problem` khi validation fail trong `src/MyProject.Application/Features/Users/AssignRolesToUser/AssignRolesToUserEndpoint.cs`

### Slice: RemoveRoleFromUser

- [ ] T038 [P] [US3] Implement `RemoveRoleFromUser` slice: `RemoveRoleFromUserCommand.cs` (Guid UserId, Guid RoleId), `RemoveRoleFromUserCommandHandler.cs` (load User với `.Include(u => u.Roles)` via `GetByIdWithRolesAsync` → 404 `User.NotFound`, kiểm tra role có trong `user.Roles` không → 404 `Role.NotAssigned` nếu không có, gọi `user.RemoveRole(role)`) trong `src/MyProject.Application/Features/Users/RemoveRoleFromUser/`
- [ ] T039 [P] [US3] Implement `RemoveRoleFromUserEndpoint.cs`: `DELETE /api/users/{id}/roles/{roleId}`, `204 No Content` thành công, `404 Problem` với title `User.NotFound` hoặc `Role.NotAssigned` trong `src/MyProject.Application/Features/Users/RemoveRoleFromUser/RemoveRoleFromUserEndpoint.cs`

**Checkpoint**: Tất cả 3 User Stories hoạt động đầy đủ. Test assign → verify → remove → verify.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Mục đích**: Kiểm tra cuối và xác nhận toàn bộ tính năng hoạt động đúng theo spec.

- [ ] T040 [P] Xác nhận toàn bộ project build thành công: `dotnet build src/MyProject.sln` — 0 errors, 0 warnings liên quan đến nullable/unused references
- [ ] T041 Chạy validation theo quickstart.md: test lần lượt tất cả 12 endpoints (5 User + 5 Role + 2 Assignment) với JWT hợp lệ; xác nhận error codes khớp với contracts (User.NotFound, Role.NotFound, User.UsernameAlreadyTaken, Role.NameAlreadyTaken, Role.HasActiveAssignments, Role.NotAssigned)

---

## Dependencies & Thứ tự thực thi

### Phase Dependencies

- **Phase 1 (Setup)**: Không có dependency — bắt đầu ngay
- **Phase 2 (Foundational)**: Phụ thuộc vào Phase 1 — **BLOCKS tất cả user stories**
- **Phase 3 (US1), Phase 4 (US2)**: Phụ thuộc vào Phase 2, có thể chạy song song sau Phase 2
- **Phase 5 (US3)**: Phụ thuộc vào Phase 2 về mặt code; cần US1 + US2 hoàn thành để test đầy đủ
- **Phase 6 (Polish)**: Phụ thuộc vào tất cả phases trước

### User Story Dependencies

- **US1 (P1)**: Có thể bắt đầu ngay sau Phase 2 — không phụ thuộc US2/US3
- **US2 (P2)**: Có thể bắt đầu ngay sau Phase 2 — không phụ thuộc US1/US3
- **US3 (P3)**: Bắt đầu sau Phase 2 — handler cần `GetByIdWithRolesAsync` (từ T013) và `GetByIdsAsync` (từ T014); cần US1+US2 để test end-to-end

### Within Each Phase 2 (Foundational)

```
Parallel batch A (T002-T009):
  T002 (User.cs)  T003 (Role.cs)  T004 (delete)  T005 (delete)
  T006 (IUser)    T007 (IRole)    T008 (delete)  T009 (delete)

Sequential after batch A:
  T010 (UserConfig — cần Role.Users từ T003)
  T011 (AppDbContext — cần UserRole deleted từ T004, T008)
  T012 (RoleRepo — cần Role.Users từ T003, UserRole deleted từ T005, T009)
  T013 (UserRepo impl — cần T006)
  T014 (RoleRepo impl — cần T007, T012)
  T015 (EF migration — cần T010, T011)
```

### Parallel Opportunities

- Tất cả tasks có `[P]` trong cùng phase có thể chạy song song
- Phase 3 (US1) và Phase 4 (US2) có thể chạy song song với nhau sau Phase 2
- Tất cả slices trong US1, US2, US3 độc lập với nhau — có thể giao cho developer khác nhau

---

## Parallel Example: Phase 2 (Foundational)

```
# Batch A — chạy cùng lúc:
T002: Cập nhật User.cs (AddRole/RemoveRole)
T003: Cập nhật Role.cs (Create/Update/Users nav)
T004: Xóa UserRole.cs
T005: Xóa IUserRoleRepository.cs
T006: Extend IUserRepository
T007: Extend IRoleRepository
T008: Xóa UserRoleConfiguration.cs
T009: Xóa UserRoleRepository.cs

# Sequential sau Batch A:
T010 → T011 → T012 → T013 → T014 → T015
```

## Parallel Example: Phase 3 (US1) — tất cả slices cùng lúc

```
T016+T017: GetUsers slice + endpoint
T018+T019: GetUserById slice + endpoint
T020+T021: CreateUser slice + endpoint
T022+T023: UpdateUser slice + endpoint
T024+T025: DeleteUser slice + endpoint
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Hoàn thành Phase 1: Setup (T001)
2. Hoàn thành Phase 2: Foundational (T002–T015) ← **CRITICAL**
3. Hoàn thành Phase 3: US1 User CRUD (T016–T025)
4. **DỪNG và VALIDATE**: Test 5 User endpoints độc lập
5. Deploy/demo nếu sẵn sàng

### Incremental Delivery

1. Setup + Foundational → Nền tảng sẵn sàng
2. US1 (User CRUD) → Test → Deploy (MVP!)
3. US2 (Role CRUD) → Test → Deploy
4. US3 (Assign/Remove) → Test → Deploy
5. Mỗi story bổ sung giá trị mà không phá vỡ stories trước

### Parallel Team Strategy

Với 2–3 developers, sau khi Phase 2 hoàn tất:
- Developer A: US1 (Phase 3) — T016–T025
- Developer B: US2 (Phase 4) — T026–T035
- Developer C: US3 (Phase 5) — T036–T039 *(sau khi US1+US2 có đủ để test)*

---

## Notes

- `[P]` tasks = khác file, không có dependency chưa hoàn thành trong phase đó
- `[US1/2/3]` label giúp trace từng task về user story tương ứng
- Mỗi slice trong Application layer gồm: Command/Query record + Handler + Validator (nếu cần) + Response DTO + Endpoint — tất cả đặt trong cùng folder
- Tham chiếu pattern từ quickstart.md cho Dapper queries, Command handlers, và Endpoint structure
- Không dùng `SELECT *` trong Dapper — luôn list explicit columns
- Error codes phải khớp với contracts/: `User.NotFound`, `Role.NotFound`, `User.UsernameAlreadyTaken`, `Role.NameAlreadyTaken`, `Role.HasActiveAssignments`, `Role.NotAssigned`
- Commit sau mỗi task hoặc nhóm logic
- Dừng tại mỗi checkpoint để validate story độc lập trước khi chuyển sang story tiếp theo
