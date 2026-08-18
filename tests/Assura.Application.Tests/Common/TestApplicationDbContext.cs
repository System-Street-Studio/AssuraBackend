using Assura.Application.Common.Interfaces;
using Assura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests.Common;

public class TestApplicationDbContext : AppDbContext
{
    public TestApplicationDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }
}
