using ProjectTemplate.Application.Abstractions.Messaging;

namespace ProjectTemplate.Application.Features.Auth.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? Birthday) : ICommand;
