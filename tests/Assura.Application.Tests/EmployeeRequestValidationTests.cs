using Assura.Application.Features.AssetRequests.Commands;
using Assura.Application.Features.Requests.Commands;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md Employee finding: "No FluentValidation on request creation
// commands — only manual null/empty-string checks." CreateRequestCommand and
// CreateAssetRequestCommand had no validators at all, so malformed input (out-of-range
// enum values, empty required fields, negative quantities) reached the handler and the
// database unchecked.
public class EmployeeRequestValidationTests
{
    private readonly CreateRequestCommandValidator _requestValidator = new();
    private readonly CreateAssetRequestCommandValidator _assetRequestValidator = new();

    [Fact]
    public void CreateRequestCommand_WithValidData_ShouldPass()
    {
        var command = new CreateRequestCommand
        {
            Type = RequestType.Asset,
            Priority = PriorityType.Medium,
            Description = "Need a laptop",
            RequesterId = 1
        };

        Assert.True(_requestValidator.Validate(command).IsValid);
    }

    [Fact]
    public void CreateRequestCommand_WithOutOfRangeEnumValues_ShouldFail()
    {
        var command = new CreateRequestCommand
        {
            Type = (RequestType)999,
            Priority = (PriorityType)999,
            RequesterId = 1
        };

        var result = _requestValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestCommand.Type));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestCommand.Priority));
    }

    [Fact]
    public void CreateRequestCommand_WithNonPositiveAssetId_ShouldFail()
    {
        var command = new CreateRequestCommand
        {
            Type = RequestType.Asset,
            Priority = PriorityType.Medium,
            RequesterId = 1,
            AssetId = 0
        };

        var result = _requestValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestCommand.AssetId));
    }

    [Fact]
    public void CreateAssetRequestCommand_WithValidData_ShouldPass()
    {
        var command = new CreateAssetRequestCommand
        {
            EmployeeId = "1",
            SubmittedBy = "Employee One",
            AssetName = "Laptop",
            Priority = "Normal",
            RequestType = "NewAsset",
            Quantity = 1
        };

        Assert.True(_assetRequestValidator.Validate(command).IsValid);
    }

    [Fact]
    public void CreateAssetRequestCommand_WithEmptyAssetName_ShouldFail()
    {
        var command = new CreateAssetRequestCommand
        {
            EmployeeId = "1",
            SubmittedBy = "Employee One",
            AssetName = "",
            Priority = "Normal",
            RequestType = "NewAsset"
        };

        var result = _assetRequestValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAssetRequestCommand.AssetName));
    }

    [Fact]
    public void CreateAssetRequestCommand_WithNegativeQuantity_ShouldFail()
    {
        var command = new CreateAssetRequestCommand
        {
            EmployeeId = "1",
            SubmittedBy = "Employee One",
            AssetName = "Laptop",
            Priority = "Normal",
            RequestType = "NewAsset",
            Quantity = -1
        };

        var result = _assetRequestValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateAssetRequestCommand.Quantity));
    }
}
