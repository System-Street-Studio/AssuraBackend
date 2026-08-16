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
            Assignments = new List<DivisionRoleAssignment> { new() { DivisionId = division.Id, Role = UserRole.Accountant.ToString() } },
            JobTitle = "Accountant",
            Notes = "Approved by HR",
            ActorName = "HR Manager",
            IpAddress = "192.168.1.5",
            Device = "Chrome"
        }, CancellationToken.None);

        var updated = await db.Users.FirstAsync(x => x.Id == user.Id);
        var auditLog = await db.AuditLogs.FirstOrDefaultAsync(x => x.Action == "Assigned Roles");

        Assert.True(result.Success);
        Assert.Empty(result.SkippedAssignments);
        Assert.Equal(UserRole.Accountant, updated.Role);
        Assert.Equal("Assigned", updated.EmploymentStatus);
        Assert.Equal("Accountant", updated.JobTitle);
        Assert.NotNull(updated.AssignedAt);
        Assert.NotNull(auditLog);
    }

    [Fact]
    public async Task AssignHrRoleCommand_CannotEscalateUserToAdminOrSystemAdmin()
    {
        using var db = CreateContext();

        var division = new Division { Id = 1, Name = "Finance" };
        var user = new User
        {
            Id = 11,
            Username = "escalation_attempt",
            FirstName = "Would",
            LastName = "BeAdmin",
            Email = "wouldbeadmin@assura.test",
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
            Assignments = new List<DivisionRoleAssignment>
            {
                new() { DivisionId = division.Id, Role = UserRole.Admin.ToString() },
                new() { DivisionId = division.Id, Role = UserRole.SystemAdmin.ToString() }
            },
            ActorName = "HR Manager"
        }, CancellationToken.None);

        var updated = await db.Users.FirstAsync(x => x.Id == user.Id);

        Assert.False(result.Success);
        Assert.Equal(2, result.SkippedAssignments.Count);
        Assert.Null(updated.Role);
        Assert.Empty(updated.DivisionRoles);
    }

    [Fact]
    public async Task AssignHrRoleCommand_PartialFailure_SkipsInvalidAssignmentsButAppliesValidOnes()
    {
        using var db = CreateContext();

        var validDivision = new Division { Id = 1, Name = "Finance" };
        var user = new User
        {
            Id = 13,
            Username = "partial_success",
            FirstName = "Partial",
            LastName = "Success",
            Email = "partial@assura.test",
            PasswordHash = "x",
            EmploymentStatus = "PendingAssignment"
        };

        db.Divisions.Add(validDivision);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new AssignHrRoleCommandHandler(db);
        var result = await handler.Handle(new AssignHrRoleCommand
        {
            UserId = user.Id,
            Assignments = new List<DivisionRoleAssignment>
            {
                new() { DivisionId = validDivision.Id, Role = UserRole.Accountant.ToString() },
                new() { DivisionId = 999, Role = UserRole.Storekeeper.ToString() }, // non-existent division
                new() { DivisionId = validDivision.Id, Role = "NotARealRole" } // invalid role
            },
            ActorName = "HR Manager"
        }, CancellationToken.None);

        var updated = await db.Users.Include(u => u.DivisionRoles).FirstAsync(x => x.Id == user.Id);

        Assert.True(result.Success);
        Assert.Equal(2, result.SkippedAssignments.Count);
        Assert.Single(updated.DivisionRoles);
        Assert.Equal(UserRole.Accountant, updated.Role);
    }

    [Fact]
    public async Task UpdateHrUserCommand_CannotEscalateUserToAdminOrSystemAdmin()
    {
        using var db = CreateContext();

        var division = new Division { Id = 1, Name = "Finance" };
        var user = new User
        {
            Id = 12,
            Username = "escalation_attempt_2",
            FirstName = "Would",
            LastName = "BeSystemAdmin",
            Email = "wouldbesysadmin@assura.test",
            PasswordHash = "x",
            Role = UserRole.Employee,
            DivisionId = division.Id,
            EmploymentStatus = "Assigned"
        };

        db.Divisions.Add(division);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new UpdateHrUserCommandHandler(db);
        await handler.Handle(new UpdateHrUserCommand
        {
            UserId = user.Id,
            Assignments = new List<DivisionRoleAssignment>
            {
                new() { DivisionId = division.Id, Role = UserRole.SystemAdmin.ToString() }
            },
            ActorName = "HR Manager"
        }, CancellationToken.None);

        var updated = await db.Users.FirstAsync(x => x.Id == user.Id);

        Assert.Equal(UserRole.Employee, updated.Role);
        Assert.Empty(updated.DivisionRoles);
    }

    [Fact]
    public async Task AssignHrRoleCommand_ValidatorRejectsEmptyAssignments()
    {
        var validator = new AssignHrRoleCommandValidator();

        var result = await validator.ValidateAsync(new AssignHrRoleCommand
        {
            UserId = 1,
            Assignments = new List<DivisionRoleAssignment>()
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Assignments");
    }

    [Fact]
    public async Task AssignHrRoleCommand_ValidatorRejectsInvalidUserIdAndDivisionId()
    {
        var validator = new AssignHrRoleCommandValidator();

        var result = await validator.ValidateAsync(new AssignHrRoleCommand
        {
            UserId = 0,
            Assignments = new List<DivisionRoleAssignment> { new() { DivisionId = 0, Role = "" } }
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("DivisionId"));
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Role"));
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
            Assignments = new List<DivisionRoleAssignment> { new() { DivisionId = newDivision.Id, Role = UserRole.HR.ToString() } },
            JobTitle = "HR Assistant",
            PhoneNumber = "0771234567",
            EmploymentStatus = "Assigned",
            Notes = "Details reviewed",
            ActorName = "HR Manager",
            IpAddress = "192.168.1.6",
            Device = "Chrome"
        }, CancellationToken.None);

        var updated = await db.Users.FirstAsync(x => x.Id == user.Id);

        Assert.True(result.Success);
        Assert.Empty(result.SkippedAssignments);
        Assert.Equal(newDivision.Id, updated.DivisionId);
        Assert.Equal(UserRole.HR, updated.Role);
        Assert.Equal("HR Assistant", updated.JobTitle);
        Assert.Equal("0771234567", updated.PhoneNumber);
        Assert.Equal("Assigned", updated.EmploymentStatus);
        Assert.Contains(db.AuditLogs, x => x.Action == "Updated Employee Details");
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
        var log = await db.AuditLogs.FirstOrDefaultAsync(x => x.Action == "Rejected Registration");

        Assert.True(result);
        Assert.False(updated.IsActive);
        Assert.Equal("Rejected", updated.EmploymentStatus);
        Assert.NotNull(log);
    }

    [Fact]
    public async Task RejectedUser_IsVisibleInRejectedListAndByIdAndCanBeReconsidered()
    {
        using var db = CreateContext();

        var user = new User
        {
            Id = 31,
            Username = "second_chance",
            FirstName = "Second",
            LastName = "Chance",
            Email = "secondchance@assura.test",
            PasswordHash = "x",
            RequestedRole = "Employee",
            EmploymentStatus = "PendingAssignment",
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        await new RejectHrUserCommandHandler(db).Handle(new RejectHrUserCommand
        {
            UserId = user.Id,
            Notes = "Incomplete documentation",
            ActorName = "HR Manager"
        }, CancellationToken.None);

        // Rejected users must remain discoverable: in the dedicated rejected list...
        var rejectedUsers = await new GetRejectedHrUsersQueryHandler(db)
            .Handle(new GetRejectedHrUsersQuery(), CancellationToken.None);
        Assert.Contains(rejectedUsers, u => u.Id == user.Id);

        // ...and individually by id (previously hidden by an IsActive filter)...
        var detail = await new GetHrUserByIdQueryHandler(db)
            .Handle(new GetHrUserByIdQuery(user.Id), CancellationToken.None);
        Assert.NotNull(detail);
        Assert.Equal("Rejected", detail!.EmploymentStatus);

        // ...and the rejection must be reversible.
        var reconsidered = await new ReconsiderHrUserCommandHandler(db)
            .Handle(new ReconsiderHrUserCommand { UserId = user.Id, ActorName = "HR Manager" }, CancellationToken.None);
        Assert.True(reconsidered);

        var reactivated = await db.Users.FirstAsync(x => x.Id == user.Id);
        Assert.True(reactivated.IsActive);
        Assert.Equal("PendingAssignment", reactivated.EmploymentStatus);

        var pendingUsers = await new GetPendingHrUsersQueryHandler(db)
            .Handle(new GetPendingHrUsersQuery(), CancellationToken.None);
        Assert.Contains(pendingUsers, u => u.Id == user.Id);
    }

    [Fact]
    public async Task ReconsiderHrUserCommand_CannotReconsiderNonRejectedUser()
    {
        using var db = CreateContext();

        var user = new User
        {
            Id = 32,
            Username = "still_pending",
            FirstName = "Still",
            LastName = "Pending",
            Email = "stillpending@assura.test",
            PasswordHash = "x",
            EmploymentStatus = "PendingAssignment"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await new ReconsiderHrUserCommandHandler(db)
            .Handle(new ReconsiderHrUserCommand { UserId = user.Id, ActorName = "HR Manager" }, CancellationToken.None);

        Assert.False(result);
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
        var assignedUsers = await new GetAssignedHrUsersQueryHandler(db).Handle(new GetAssignedHrUsersQuery(Role: "Accountant"), CancellationToken.None);
        var activityLogs = await new GetHrActivityLogsQueryHandler(db).Handle(new GetHrActivityLogsQuery("John"), CancellationToken.None);
        var userDetail = await new GetHrUserByIdQueryHandler(db).Handle(new GetHrUserByIdQuery(42), CancellationToken.None);

        Assert.Contains(overview.Stats, x => x.Label == "Pending Users" && x.Value == 1);
        Assert.Single(pendingUsers);
        Assert.Single(assignedUsers);
        Assert.Single(activityLogs);
        Assert.NotNull(userDetail);
        Assert.Equal("Finance", userDetail!.Division);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
