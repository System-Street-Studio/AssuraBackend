using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

using Assura.Application.Common.Interfaces;

using Assura.Domain.Entities;

using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;



namespace Assura.Infrastructure.Identity;



public class JwtTokenGenerator : IJwtTokenGenerator

{

    private readonly IConfiguration _configuration;



    public JwtTokenGenerator(IConfiguration configuration)

    {

        _configuration = configuration;

    }



    public string GenerateToken(User user)
    {
        return GenerateToken(user, null, null);
    }

    public string GenerateToken(User user, string? activeRole, int? activeDivisionId)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings.GetValue<string>("Key") ?? "YourDevelopmentSecretKeyChangeInProduction";

        var sessionId = user.CurrentSessionId ?? Guid.NewGuid().ToString();

        var roleSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (user.Role.HasValue)
        {
            roleSet.Add(user.Role.Value.ToString());
        }
        if (user.DivisionRoles != null)
        {
            foreach (var dr in user.DivisionRoles)
            {
                roleSet.Add(dr.Role.ToString());
            }
        }

        if (!string.IsNullOrWhiteSpace(activeRole))
        {
            roleSet.Add(activeRole);
        }

        var effectiveDivisionId = activeDivisionId ?? user.DivisionId;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Explicit mapping for ASP.NET Core controllers
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("DivisionId", effectiveDivisionId?.ToString() ?? ""),
            new("RequiresOnboarding", user.RequiresOnboarding ? "true" : "false"),
            new("SessionId", sessionId),
            new(JwtRegisteredClaimNames.Jti, sessionId)
        };

        if (!string.IsNullOrWhiteSpace(activeRole))
        {
            claims.Add(new Claim("ActiveRole", activeRole));
        }

        // Put active role FIRST so User.FindFirst(ClaimTypes.Role) and frontend getRole() return the active role
        var orderedRoles = new List<string>();
        if (!string.IsNullOrWhiteSpace(activeRole) && roleSet.Contains(activeRole))
        {
            orderedRoles.Add(activeRole);
        }
        foreach (var role in roleSet)
        {
            if (!orderedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                orderedRoles.Add(role);
            }
        }

        if (orderedRoles.Count == 0)
        {
            claims.Add(new Claim(ClaimTypes.Role, string.Empty));
        }
        else
        {
            foreach (var role in orderedRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryMinutes", 60));

        var token = new JwtSecurityToken(
            issuer: jwtSettings.GetValue<string>("Issuer"),
            audience: jwtSettings.GetValue<string>("Audience"),
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}

