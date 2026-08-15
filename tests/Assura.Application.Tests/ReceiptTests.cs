using Assura.Application.Common.Behaviors;
using Assura.Application.Features.Receipts.Commands.Create;
using Assura.Application.Features.Receipts.Commands.UploadFile;
using Assura.Application.Tests.Common;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Assura.Application.Tests;

public class ReceiptTests
{
    private readonly CreateReceiptCommandValidator _validator = new();

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(Assura.Application.DependencyInjection).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void Validator_WithValidCommand_ShouldPass()
    {
        var command = new CreateReceiptCommand
        {
            AssetName = "MacBook Pro 16",
            Division = "Engineering",
            Date = "15 Aug 2026",
            Amount = 2500.00m
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Engineering", "15 Aug 2026", 100)]
    [InlineData("MacBook", "", "15 Aug 2026", 100)]
    [InlineData("MacBook", "Engineering", "not-a-date", 100)]
    [InlineData("MacBook", "Engineering", "15 Aug 2026", 0)]
    [InlineData("MacBook", "Engineering", "15 Aug 2026", -50)]
    public void Validator_WithInvalidCommand_ShouldFail(string assetName, string division, string date, decimal amount)
    {
        var command = new CreateReceiptCommand
        {
            AssetName = assetName,
            Division = division,
            Date = date,
            Amount = amount
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Handler_ShouldPersistAmountAsDecimal()
    {
        using var context = TestContextFactory.CreateContext();
        var handler = new CreateReceiptCommandHandler(context);

        var command = new CreateReceiptCommand
        {
            AssetName = "Dell XPS 15",
            Division = "Engineering",
            Date = "10 Aug 2026",
            Amount = 2200.55m
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(2200.55m, result.Amount);
        var stored = context.Receipts.Single();
        Assert.Equal(2200.55m, stored.Amount);
    }

    [Fact]
    public async Task Behavior_ShouldRejectZeroAmount_BeforeReachingHandler()
    {
        using var context = TestContextFactory.CreateContext();
        var handler = new CreateReceiptCommandHandler(context);
        var behavior = new ValidationBehavior<CreateReceiptCommand, Assura.Application.Features.Receipts.DTOs.ReceiptDto>(
            new[] { _validator });

        var command = new CreateReceiptCommand
        {
            AssetName = "Dell XPS 15",
            Division = "Engineering",
            Date = "10 Aug 2026",
            Amount = 0
        };

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None));
    }

    [Fact]
    public async Task UploadFileHandler_ShouldAttachFileUrl_AndMarkUploaded()
    {
        using var context = TestContextFactory.CreateContext();
        var mapper = CreateMapper();
        var createHandler = new CreateReceiptCommandHandler(context);
        var created = await createHandler.Handle(new CreateReceiptCommand
        {
            AssetName = "Epson Projector",
            Division = "Sales",
            Date = "01 Aug 2026",
            Amount = 1500m
        }, CancellationToken.None);

        var uploadHandler = new UploadReceiptFileCommandHandler(context, mapper);
        var result = await uploadHandler.Handle(new UploadReceiptFileCommand(created.Id, "/uploads/receipts/test.pdf"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("/uploads/receipts/test.pdf", result!.FileUrl);
        Assert.Equal("Uploaded", result.Status);
    }

    [Fact]
    public async Task UploadFileHandler_ShouldReturnNull_WhenReceiptDoesNotExist()
    {
        using var context = TestContextFactory.CreateContext();
        var mapper = CreateMapper();
        var uploadHandler = new UploadReceiptFileCommandHandler(context, mapper);

        var result = await uploadHandler.Handle(new UploadReceiptFileCommand("does-not-exist", "/uploads/receipts/test.pdf"), CancellationToken.None);

        Assert.Null(result);
    }
}
