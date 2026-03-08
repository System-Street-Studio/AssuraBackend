using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Infrastructure.Identity;

public class IdentityService : IIdentifyServices
{
    private readonly IApplicationDbContext _context;

    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
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

    public async Task<Assura.Application.Common.Models.AuthResponse?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new Assura.Application.Common.Models.AuthResponse
        {
            Token = token,
            User = new Assura.Application.Common.Models.UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = $"{user.FirstName} {user.LastName}",
                Roles = new List<string> { user.Role?.ToString() ?? "Employee" }
            }
        };
    }
}
