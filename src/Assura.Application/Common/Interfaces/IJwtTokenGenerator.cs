using Assura.Domain.Entities;

namespace Assura.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
