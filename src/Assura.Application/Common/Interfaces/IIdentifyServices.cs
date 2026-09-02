// basic rules in auth system

using Assura.Application.Common.Models;
using Assura.Domain.Enums;

namespace Assura.Application.Common.Interfaces;

public interface IIdentifyServices
{
    Task<AuthResponse?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterAsync(
        string username,
        string password,
        string email,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        string? requestedRole = null,
        int? divisionId = null);
    Task<bool> UserExistsAsync(string username, string email);
    /// <summary>Returns a specific conflict message if username, email, or password is taken/invalid, or null if clear.</summary>
    Task<string?> CheckUserConflictAsync(string username, string email, string? password = null);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task LogoutAsync(int userId);

    /// <summary>Re-issues a JWT for a user whose claims just changed (e.g. after completing
    /// onboarding), so the caller doesn't have to log in again to see the updated token.</summary>
    Task<string?> RegenerateTokenAsync(int userId);
}
