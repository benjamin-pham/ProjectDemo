using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Auth.Register;

/// <summary>
/// model đăng ký tài khoản
/// </summary>
/// <param name="FirstName">Tên</param>
/// <param name="LastName">Họ</param>
/// <param name="Username">Tên người dùng</param>
/// <param name="Password">Mật khẩu</param>
/// <param name="Email">Thư điện tử</param>
/// <param name="Phone">Điện thoại</param>
/// <param name="Birthday">Sinh nhật</param>
public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Username,
    string Password,
    string? Email,
    string? Phone,
    DateOnly? Birthday) : ICommand<RegisterUserResponse>;


