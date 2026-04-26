using Assura.Application.Features.HR.Commands;
using Assura.Application.Features.HR.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

public class HrModuleTests
{
    [Fact]
    public async Task AssignHrRoleCommand_AssignsRoleAndCreatesAuditLog()
    {
        using var db = CreateContext();

        var division = new Division { Id = 1, Name = "Finance" };
        var user = new User
        {
            Id = 10,
            Username = "john_p",
            FirstName = "John",
            LastName = "Pereira",
            Email = "john@assura.test",
            PasswordHash = "x",
            EmploymentStatus = "PendingAssignment"
        };

        db.Divisions.Add(division);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new AssignHrRoleCommandHandler(db);
        var result = await handler.Handle(new AssignHrRoleCommand
        {
            UserId = user.Id,
            Role = UserRole.Accountant.ToString(),
            DivisionId = division.Id,
            JobTitle = "Accountant",
            Notes = "Approved by HR",
            ActorName = "HR Manager",
            IpAddress = "192.168.1.5",
            Device = "Chrome"
        }, CancellationToken.None);

        var updated = await db.Users.FirstAsync(x => x.Id == user.Id);
        var auditLog = await db.AuditLogs.FirstAsync();

        Assert.True(result);
        Assert.Equal(UserRole.Accountant, updated.Role);
        Assert.Equal("Assigned", updated.EmploymentStatus);
        Assert.Equal("Accountant", updated.JobTitle);
        Assert.NotNull(updated.AssignedAt);
        Assert.Equal("Assigned Role", auditLog.Action);
    }

    [Fact]
    public async Task UpdateHrUserCommand_UpdatesUserAndCreatesAuditLog()
    {
        using var db = CreateContext();

        var oldDivision = new Division { Id = 1, Name = "Operations" };
        var newDivision = new Division { Id = 2, Name = "HR" };
        var user = new User
        {
            Id = 20,
            Username = "amanda_lee",
            FirstName = "Amanda",
            LastName = "Lee",
            Email = "amanda@assura.test",
            PasswordHash = "x",
            DivisionId = oldDivision.Id,
            Role = UserRole.Employee,
            EmploymentStatus = "PendingAssignment"
        };

        db.Divisions.AddRange(oldDivision, newDivision);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new UpdateHrUserCommandHandler(db);
        var result = await handler.Handle(new UpdateHrUserCommand
        {
            UserId = user.Id,
            DivisionId = newDivision.Id,
            Role = UserRole.HR.ToString(),
            JobTitle = "HR Assistant",
            PhoneNumber = "0771234567",
            EmploymentStatus = "Assigned",
            Notes = "Details reviewed",
            ActorName = "HR Manager",
            IpAddress = "192.168.1.6",
            Device = "Chrome"
        }, CancellationToken.None);

        var updated = await db.Users.FirstAsync(x => x.Id == user.Id);

        Assert.True(result);
        Assert.Equal(newDivision.Id, updated.DivisionId);
        Assert.Equal(UserRole.HR, updated.Role);
        Assert.Equal("HR Assistant", updated.JobTitle);
        Assert.Equal("0771234567", updated.PhoneNumber);
        Assert.Equal("Assigned", updated.EmploymentStatus);
        Assert.Single(db.AuditLogs);
    }

    [Fact]
    public async Task RejectHrUserCommand_MarksUserRejectedAndInactive()
    {
        using var db = CreateContext();

        var user = new User
        {
            Id = 30,
            Username = "temp_user",
            FirstName = "Temp",
            LastName = "User",
            Email = "temp@assura.test",
            PasswordHash = "x",
            RequestedRole = "Employee",
            EmploymentStatus = "PendingAssignment"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new RejectHrUserCommandHandler(db);
        var result = await handler.Handle(new RejectHrUserCommand
        {
            UserId = user.Id,
            Notes = "Incomplete documentation",
            ActorName = "HR Manager",
            IpAddress = "192.168.1.8",
            Device = "Chrome"
        }, CancellationToken.None);

        var updated = await db.Users.IgnoreQueryFilters().FirstAsync(x => x.Id == user.Id);
        var log = await db.AuditLogs.FirstAsync();

        Assert.True(result);
        Assert.False(updated.IsActive);
        Assert.Equal("Rejected", updated.EmploymentStatus);
        Assert.Equal("Rejected Registration", log.Action);
    }

    [Fact]
    public async Task HrQueries_ReturnExpectedDashboardAndFilteredLists()
    {
        using var db = CreateContext();

        var hr = new Division { Id = 1, Name = "HR" };
        var finance = new Division { Id = 2, Name = "Finance" };

        db.Divisions.AddRange(hr, finance);
        db.Users.AddRange(
            new User
            {
                Id = 41,
                Username = "pending_user",
                FirstName = "Pending",
                LastName = "User",
                Email = "pending@assura.test",
                PasswordHash = "x",
                DivisionId = hr.Id,
                RequestedRole = "HR",
                EmploymentStatus = "PendingAssignment",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 42,
                Username = "john_p",
                FirstName = "John",
                LastName = "Pereira",
                Email = "john@assura.test",
                PasswordHash = "x",
                DivisionId = finance.Id,
                Role = UserRole.Accountant,
                JobTitle = "Accountant",
                EmploymentStatus = "Assigned",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 43,
                Username = "aruni_hr",
                FirstName = "Aruni",
                LastName = "Perera",
                Email = "aruni@assura.test",
                PasswordHash = "x",
                DivisionId = hr.Id,
                Role = UserRole.HR,
                JobTitle = "HR Assistant",
                EmploymentStatus = "Assigned",
                CreatedAt = DateTime.UtcNow
            });

        db.AuditLogs.Add(new AuditLog
        {
            EntityName = "HR",
            EntityId = "42",
            Action = "Assigned Role",
            CreatedBy = "HR Manager",
            IpAddress = "192.168.1.5",
            NewValues = "{\"employee\":\"John Pereira\",\"department\":\"Finance\",\"role\":\"Accountant\",\"notes\":\"Role assigned after approval\",\"result\":\"Success\",\"device\":\"Chrome\"}"
        });

        await db.SaveChangesAsync();

        var overview = await new GetHrOverviewQueryHandler(db).Handle(new GetHrOverviewQuery(), CancellationToken.None);
        var pendingUsers = await new GetPendingHrUsersQueryHandler(db).Handle(new GetPendingHrUsersQuery("pending"), CancellationToken.None);
        var assignedUsers = await new GetAssignedHrUsersQueryHandler(db).Handle(new GetAssignedHrUsersQuery(role: "Accountant"), CancellationToken.None);
        var activityLogs = await new GetHrActivityLogsQueryHandler(db).Handle(new GetHrActivityLogsQuery("John"), CancellationToken.None);
        var userDetail = await new GetHrUserByIdQueryHandler(db).Handle(new GetHrUserByIdQuery(42), CancellationToken.None);

        Assert.Contains(overview.Stats, x => x.Label == "Pending Users" && x.Value == 1);
        Assert.Single(pendingUsers);
        Assert.Single(assignedUsers);
        Assert.Single(activityLogs);
        Assert.NotNull(userDetail);
        Assert.Equal("Finance", userDetail!.Division);
    }

    private static TestApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }
}
