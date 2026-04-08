using System;

namespace MyProject.Application.Features.Auth.GetProfile;
/// <summary>
/// 
/// </summary>
/// <param name="UserId"></param>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="Username"></param>
/// <param name="Email"></param>
/// <param name="Phone"></param>
/// <param name="Birthday"></param>
/// <param name="CreatedAt"></param>
public sealed record UserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Username,
    string? Email,
    string? Phone,
    DateOnly? Birthday,
    DateTime CreatedAt);
