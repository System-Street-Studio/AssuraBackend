// basic rules in auth system

using Assura.Application.Common.Models;
using Assura.Domain.Enums;

namespace Assura.Application.Common.Interfaces;

public interface IIdentifyServices
{
    Task<AuthResponse?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string password, string email, string firstName, string lastName);
    Task<bool> UserExistsAsync(string username, string email);
}