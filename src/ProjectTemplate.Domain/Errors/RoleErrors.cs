namespace ProjectTemplate.Domain.Errors;

using ProjectTemplate.Domain.Abstractions;

public static class RoleErrors
{
    public static readonly Error NotFound =
        new("Role.NotFound", "Vai trò không tồn tại.");

    public static readonly Error SomeNotFound =
        new("Role.NotFound", "Một hoặc nhiều vai trò không tồn tại.");

    public static readonly Error NameAlreadyTaken =
        new("Role.NameAlreadyTaken", "Tên vai trò đã được sử dụng.");

    public static readonly Error HasActiveAssignments =
        new("Role.HasActiveAssignments", "Không thể xóa vai trò đang được gán cho người dùng.");

    public static readonly Error NotAssigned =
        new("Role.NotAssigned", "Vai trò không được gán cho người dùng này.");
}
