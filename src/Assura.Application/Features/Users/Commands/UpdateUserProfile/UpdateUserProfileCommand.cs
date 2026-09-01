using Assura.Application.Common.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;



namespace Assura.Application.Features.Users.Commands.UpdateUserProfile;



public record UpdateProfileResult(bool Success, string ErrorMessage = "");

public record UpdateUserProfileCommand : IRequest<UpdateProfileResult>
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? CurrentPassword { get; set; }
    public string? Password { get; set; }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateProfileResult>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateProfileResult> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) return new UpdateProfileResult(false, "User not found.");

        user.Username = request.Username;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrEmpty(request.Password))
        {
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            if (!passwordRegex.IsMatch(request.Password))
            {
                return new UpdateProfileResult(false, "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            }

            if (string.Equals(request.Password, request.Username, StringComparison.OrdinalIgnoreCase))
            {
                return new UpdateProfileResult(false, "Password must not be the same as the username.");
            }

            if (string.IsNullOrEmpty(request.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return new UpdateProfileResult(false, "Invalid current password.");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new UpdateProfileResult(true);
    }
}

