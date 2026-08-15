using Assura.Application.Features.DiscardedNotes.Commands.UpdateStatus;
using Assura.Application.Features.QueueItems.Commands.UpdateStatus;

namespace Assura.Application.Tests;

// Covers the BUGS.md Superintendent finding: "UpdateQueueItemStatusCommand /
// UpdateDiscardedNoteStatusCommand silently no-op on an invalid Status string and
// still report success." Enum.TryParse failure used to leave the entity's status
// unchanged while the handler still returned true (204). These tests assert the new
// validators reject an invalid status string before the handler ever runs, and still
// accept every real enum value.
public class QueueAndDiscardedNoteStatusValidationTests
{
    [Theory]
    [InlineData("NotARealStatus")]
    [InlineData("")]
    public void UpdateQueueItemStatusCommandValidator_ShouldReject_InvalidStatus(string status)
    {
        var validator = new UpdateQueueItemStatusCommandValidator();
        var result = validator.Validate(new UpdateQueueItemStatusCommand { Id = 1, Status = status });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Approved")]
    [InlineData("Discarded")]
    [InlineData("approved")]
    public void UpdateQueueItemStatusCommandValidator_ShouldAccept_ValidStatus(string status)
    {
        var validator = new UpdateQueueItemStatusCommandValidator();
        var result = validator.Validate(new UpdateQueueItemStatusCommand { Id = 1, Status = status });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("NotARealStatus")]
    [InlineData("")]
    public void UpdateDiscardedNoteStatusCommandValidator_ShouldReject_InvalidStatus(string status)
    {
        var validator = new UpdateDiscardedNoteStatusCommandValidator();
        var result = validator.Validate(new UpdateDiscardedNoteStatusCommand { Id = 1, Status = status, Note = "" });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Completed")]
    [InlineData("completed")]
    public void UpdateDiscardedNoteStatusCommandValidator_ShouldAccept_ValidStatus(string status)
    {
        var validator = new UpdateDiscardedNoteStatusCommandValidator();
        var result = validator.Validate(new UpdateDiscardedNoteStatusCommand { Id = 1, Status = status, Note = "" });

        Assert.True(result.IsValid);
    }
}
