using Assura.Application.Features.Buyers.Commands.Create;

namespace Assura.Application.Tests;

public class BuyerCommandValidationTests
{
    private readonly CreateBuyerCommandValidator _validator = new();

    [Theory]
    [InlineData("0712345678")]
    [InlineData("0771234567")]
    [InlineData("0112345678")]
    public void Validator_ShouldPass_ForValidBuyerCommand(string phone)
    {
        var command = new CreateBuyerCommand(
            "Techno Supplies Ltd",
            "John Perera",
            "info@techno.lk",
            phone,
            "Electronics"
        );

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")] // less than 2 chars
    public void Validator_ShouldReject_InvalidName(string name)
    {
        var command = new CreateBuyerCommand(
            name,
            "John Perera",
            "info@techno.lk",
            "0712345678",
            "Electronics"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBuyerCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")] // less than 2 chars
    public void Validator_ShouldReject_InvalidContact(string contact)
    {
        var command = new CreateBuyerCommand(
            "Techno Supplies Ltd",
            contact,
            "info@techno.lk",
            "0712345678",
            "Electronics"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBuyerCommand.Contact));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@invalid.com")]
    public void Validator_ShouldReject_InvalidEmail(string email)
    {
        var command = new CreateBuyerCommand(
            "Techno Supplies Ltd",
            "John Perera",
            email,
            "0712345678",
            "Electronics"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBuyerCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567890")] // does not start with 0
    [InlineData("071234567")]   // 9 digits (too short)
    [InlineData("07123456789")] // 11 digits (too long)
    [InlineData("071-2345678")] // contains hyphen
    [InlineData("071 2345678")] // contains space
    [InlineData("071234567a")] // contains letter
    public void Validator_ShouldReject_InvalidPhone(string phone)
    {
        var command = new CreateBuyerCommand(
            "Techno Supplies Ltd",
            "John Perera",
            "info@techno.lk",
            phone,
            "Electronics"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBuyerCommand.Phone));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")] // less than 2 chars
    public void Validator_ShouldReject_InvalidCategory(string category)
    {
        var command = new CreateBuyerCommand(
            "Techno Supplies Ltd",
            "John Perera",
            "info@techno.lk",
            "0712345678",
            category
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBuyerCommand.Category));
    }
}

public class UpdateBuyerCommandValidationTests
{
    private readonly Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommandValidator _validator = new();

    [Fact]
    public void UpdateValidator_ShouldPass_ForValidCommand()
    {
        var command = new Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand(
            1,
            "Updated Supplies Ltd",
            "Jane Silva",
            "contact@updated.lk",
            "0771234567",
            "Machinery",
            "Active"
        );

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdateValidator_ShouldReject_InvalidId(int id)
    {
        var command = new Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand(
            id,
            "Updated Supplies Ltd",
            "Jane Silva",
            "contact@updated.lk",
            "0771234567",
            "Machinery",
            "Active"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand.Id));
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("071234567")]
    [InlineData("07123456789")]
    [InlineData("071-2345678")]
    public void UpdateValidator_ShouldReject_InvalidPhone(string phone)
    {
        var command = new Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand(
            1,
            "Updated Supplies Ltd",
            "Jane Silva",
            "contact@updated.lk",
            phone,
            "Machinery",
            "Active"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand.Phone));
    }

    [Theory]
    [InlineData("UnknownStatus")]
    [InlineData("Disabled")]
    public void UpdateValidator_ShouldReject_InvalidStatus(string status)
    {
        var command = new Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand(
            1,
            "Updated Supplies Ltd",
            "Jane Silva",
            "contact@updated.lk",
            "0771234567",
            "Machinery",
            status
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Assura.Application.Features.Buyers.Commands.Update.UpdateBuyerCommand.Status));
    }
}
