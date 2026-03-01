using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Infrastructure.Identity;

public class IdentityService : IIdentifyServices
{
    private readonly IApplicationDbContext _context;

    public IdentityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RegisterAsync(string username, string password, string email, string firstName, string lastName)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            // Role and DivisionID are assigned later by HR
            IsActive = true,
            IsLocked = false
        };

        _context.Users.Add(user);
        return await _context.SaveChangesAsync(default) > 0;
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
    }

    public Task<Assura.Application.Common.Models.AuthResponse> AuthenticateAsync(string username, string password)
    {
        // Placeholder for now
        return Task.FromResult<Assura.Application.Common.Models.AuthResponse>(null!);
    }
}
