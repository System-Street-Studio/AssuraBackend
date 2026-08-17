using Assura.Application.Common.Behaviors;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Reporting.Commands;
using Assura.Application.Tests.Common;
using FluentValidation;
using Moq;

namespace Assura.Application.Tests;

public class CreateCustomReportCommandTests
{
    private readonly CreateCustomReportCommandValidator _validator = new();

    [Fact]
    public void Validator_WithValidNonScheduledCommand_ShouldPass()
    {
        var command = new CreateCustomReportCommand
        {
            Title = "Monthly Asset Report",
            Type = "Audit",
            IsScheduled = false
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_WithValidScheduledCommand_ShouldPass()
    {
        var command = new CreateCustomReportCommand
        {
            Title = "Monthly Asset Report",
            Type = "Audit",
            IsScheduled = true,
            ScheduleFrequency = "Weekly"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_WithEmptyTitle_ShouldFail()
    {
        var command = new CreateCustomReportCommand { Title = "", Type = "Audit" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomReportCommand.Title));
    }

    [Fact]
    public void Validator_WithEmptyType_ShouldFail()
    {
        var command = new CreateCustomReportCommand { Title = "Monthly Report", Type = "" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomReportCommand.Type));
    }

    [Fact]
    public void Validator_WithScheduledAndInvalidFrequency_ShouldFail()
    {
        var command = new CreateCustomReportCommand
        {
            Title = "Monthly Report",
            Type = "Audit",
            IsScheduled = true,
            ScheduleFrequency = "Fortnightly"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomReportCommand.ScheduleFrequency));
    }

    [Fact]
    public void Validator_WithScheduledAndMissingFrequency_ShouldFail()
    {
        var command = new CreateCustomReportCommand
        {
            Title = "Monthly Report",
            Type = "Audit",
            IsScheduled = true,
            ScheduleFrequency = null
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomReportCommand.ScheduleFrequency));
    }

    [Fact]
    public void Validator_WithUnscheduledAndNoFrequency_ShouldPass()
    {
        var command = new CreateCustomReportCommand
        {
            Title = "Monthly Report",
            Type = "Audit",
            IsScheduled = false,
            ScheduleFrequency = null
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Pipeline_WithEmptyTitle_ShouldThrowValidationException_BeforeHandlerRuns()
    {
        using var db = TestContextFactory.CreateContext();
        var mockCurrentUser = new Mock<ICurrentUserService>();
        mockCurrentUser.Setup(m => m.UserId).Returns("1");
        var handler = new CreateCustomReportCommandHandler(db, mockCurrentUser.Object);

        var behavior = new ValidationBehavior<CreateCustomReportCommand, string>(new[] { _validator });

        var command = new CreateCustomReportCommand { Title = "", Type = "Audit" };

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None));

        Assert.Empty(db.CustomReports);
    }
}
