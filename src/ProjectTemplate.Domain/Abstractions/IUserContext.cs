namespace ProjectTemplate.Domain.Abstractions;

public interface IUserContext
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}
