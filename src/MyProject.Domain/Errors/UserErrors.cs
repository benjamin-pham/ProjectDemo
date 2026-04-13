namespace MyProject.Domain.Errors;

using MyProject.Domain.Abstractions;

public static class UserErrors
{
    public static readonly Error NotFound =
        new("User.NotFound", "Người dùng không tồn tại.");

    public static readonly Error UsernameAlreadyTaken =
        new("User.UsernameAlreadyTaken", "Username đã được sử dụng.");

    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Thông tin đăng nhập không hợp lệ.");

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Refresh token không hợp lệ hoặc đã hết hạn.");
}
