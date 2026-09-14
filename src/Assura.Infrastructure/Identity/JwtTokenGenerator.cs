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
        // Operational users also have access to the Employee portal
        if (!roleSet.Contains("Pending") && !roleSet.Contains("SystemAdmin") && roleSet.Count > 0)
        {
            roleSet.Add("Employee");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Explicit mapping for ASP.NET Core controllers
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("DivisionId", user.DivisionId?.ToString() ?? ""),
            new("RequiresOnboarding", user.RequiresOnboarding ? "true" : "false"),
            new("SessionId", sessionId),
            new(JwtRegisteredClaimNames.Jti, sessionId)
        };

        if (roleSet.Count == 0)
        {
            claims.Add(new Claim(ClaimTypes.Role, string.Empty));
        }
        else
        {
            foreach (var role in roleSet)
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

