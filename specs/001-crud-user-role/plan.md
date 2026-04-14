# Implementation Plan: CRUD API cho User và Role

**Branch**: `001-crud-user-role` | **Ngày**: 2026-04-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-crud-user-role/spec.md`

## Tóm tắt

Xây dựng bộ CRUD API đầy đủ cho hai thực thể `User` và `Role` theo mô hình Clean Architecture + MediatR CQRS đang được áp dụng trong dự án. Tính năng bao gồm 5 endpoints quản lý User (GET list, GET by id, POST, PUT, DELETE), 5 endpoints quản lý Role, và 2 endpoints gán/gỡ bỏ vai trò. Tất cả endpoints yêu cầu xác thực JWT. Song song đó, cần refactor `Role.cs` từ mô hình join entity tường minh (`UserRole`) sang EF Core implicit many-to-many (`User.Roles` ↔ `Role.Users`) theo đặc tả đã xác nhận.

## Technical Context

**Language/Version**: C# 13 / .NET 10  
**Primary Dependencies**: MediatR 12.4.1, FluentValidation latest, EF Core 10.x (Npgsql), Dapper latest, Serilog latest  
**Storage**: PostgreSQL — bảng `users`, `roles`, `user_roles` (snake_case, EF Fluent API)  
**Testing**: xUnit + NSubstitute + FluentAssertions (unit) · xUnit + Testcontainers + Respawn (integration, PostgreSQL thực)  
**Target Platform**: Linux server (Docker container)  
**Project Type**: Web service — ASP.NET Core 10 Minimal API, class-per-endpoint pattern  
**Performance Goals**: Phản hồi trang đầu tiên danh sách User/Role < 2 giây với 10.000+ bản ghi  
**Constraints**: < 200ms p95 cho các thao tác đơn lẻ; phân trang mặc định 20 bản ghi/trang  
**Scale/Scope**: ~10.000 người dùng ban đầu; tăng trưởng xử lý qua Dapper + OFFSET/LIMIT pagination

## Constitution Check

*GATE: Phải pass trước Phase 0. Re-check sau Phase 1 design.*

Xác minh dựa trên MyProject Constitution v1.0.0:

- [x] **I. Clean Architecture** — Tất cả Commands/Queries nằm trong `Application`; EF Core chỉ trong `Infrastructure`; endpoints trong `Application/Features/**/*Endpoint.cs` chỉ gọi `ISender.Send()`. Luồng phụ thuộc đúng hướng: `API → Application → Domain`, `Infrastructure → Application → Domain`.
- [x] **II. Rich Domain Model** — `User` đã có `static Create(...)` factory và `UpdateProfile(...)` method. `Role` cần bổ sung `static Create(...)` factory (đã xác nhận trong clarification). State mutation cho gán/gỡ role thực hiện qua `User.AddRole(role)` / `User.RemoveRole(role)`.
- [x] **III. .NET 10 Practices** — File-scoped namespaces, records cho Command/Query/Response DTOs, primary constructors cho handlers. Methods ≤ 30 lines, classes ≤ 200 lines. Nullable enabled.
- [x] **IV. Testing Discipline** — Unit tests cho Domain entities và Application handlers; integration tests dùng Testcontainers PostgreSQL thực. Không dùng in-memory DB.
- [x] **V. Observability** — Serilog structured logging trong tất cả handlers qua `LoggingBehavior` pipeline đã có sẵn. Correlation ID được xử lý bởi `CorrelationIdMiddleware`. Không log password hash, token.
- [x] **Tech Stack** — Không thêm dependency mới; tất cả công nghệ đã có trong constitution.

## Project Structure

### Documentation (tính năng này)

```text
specs/001-crud-user-role/
├── plan.md              # File này (/speckit.plan output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── users.md
│   ├── roles.md
│   └── user-roles.md
└── tasks.md             # Phase 2 output (/speckit.tasks — chưa tạo)
```

### Source Code (cấu trúc thực tế sau khi implement)

```text
src/
├── MyProject.Domain/
│   ├── Entities/
│   │   ├── User.cs                      # Thêm AddRole(), RemoveRole()
│   │   └── Role.cs                      # Thêm Create() factory, đổi UserRoles → Users nav
│   ├── Repositories/
│   │   ├── IUserRepository.cs           # Thêm GetPagedAsync(), ExistsByUsernameAsync()
│   │   └── IRoleRepository.cs           # Thêm GetPagedAsync(), ExistsByNameAsync()
│   └── Enumerations/
│       └── RoleType.cs                  # Không đổi
│
├── MyProject.Application/
│   └── Features/
│       ├── Users/
│       │   ├── GetUsers/                # Query + Handler + Endpoint (Dapper)
│       │   ├── GetUserById/             # Query + Handler + Endpoint (Dapper)
│       │   ├── CreateUser/              # Command + Handler + Validator + Endpoint
│       │   ├── UpdateUser/              # Command + Handler + Validator + Endpoint
│       │   ├── DeleteUser/              # Command + Handler + Endpoint
│       │   ├── AssignRolesToUser/       # Command + Handler + Validator + Endpoint
│       │   └── RemoveRoleFromUser/      # Command + Handler + Endpoint
│       └── Roles/
│           ├── GetRoles/                # Query + Handler + Endpoint (Dapper)
│           ├── GetRoleById/             # Query + Handler + Endpoint (Dapper)
│           ├── CreateRole/              # Command + Handler + Validator + Endpoint
│           ├── UpdateRole/              # Command + Handler + Validator + Endpoint
│           └── DeleteRole/             # Command + Handler + Endpoint
│
├── MyProject.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs    # Cập nhật HasMany().WithMany() implicit
│   │   │   ├── RoleConfiguration.cs    # Không đổi
│   │   │   └── UserRoleConfiguration.cs # XÓA — thay bằng implicit many-to-many
│   │   ├── Migrations/
│   │   │   └── [timestamp]_RefactorImplicitManyToMany.cs  # EF migration mới
│   │   └── AppDbContext.cs             # Xóa DbSet<UserRole>, thêm DbSet<Role>
│   └── Repositories/
│       ├── UserRepository.cs           # Thêm GetPagedAsync()
│       └── RoleRepository.cs          # Thêm GetPagedAsync(), cập nhật GetByUserIdAsync()
│
└── MyProject.WebHost/ (không thay đổi cấu trúc — endpoints tự đăng ký qua IEndpoint)
```

**Structure Decision**: Option 3 — Clean Architecture với vertical slice (feature-first) trong Application layer. Cấu trúc này đang được áp dụng trong project (xem `Features/Auth/`). Tính năng mới đặt trong `Features/Users/` và `Features/Roles/`.

## Complexity Tracking

> Điền khi Constitution Check có vi phạm cần justification

| Vi phạm | Lý do cần thiết | Tại sao không thể đơn giản hơn |
|---------|-----------------|-------------------------------|
| Refactor `UserRole` explicit → implicit many-to-many | Spec clarification yêu cầu `Role.Users` nav; `UserRoleRepository` và config hiện tại conflict với nav mới | Nếu giữ explicit entity thì phải giữ `UserRoles` nav trên `Role` và `UserRoleRepository` — mâu thuẫn với spec đã xác nhận |
| Thêm `AddRole()`/`RemoveRole()` vào `User` entity | Rich domain model (Constitution II) — mutation qua named methods | Nếu gán trực tiếp `user.Roles.Add(role)` từ handler sẽ bypass domain invariants và vi phạm Constitution II |
