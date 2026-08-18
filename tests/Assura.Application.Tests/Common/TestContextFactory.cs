using Assura.Application.Common.Interfaces;
using Assura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Assura.Application.Tests.Common;

public static class TestContextFactory
{
    public static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(m => m.UserId).Returns("1");

        return new TestApplicationDbContext(options, mockUserService.Object);
    }
}
