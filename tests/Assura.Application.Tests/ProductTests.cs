using Assura.Application.DTOs;
using Assura.Application.Features.Products.Commands;
using Assura.Application.Features.Products.Queries;
using Assura.Application.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assura.Application.Tests;

public class ProductTests
{
    [Fact]
    public async Task CreateProductCommandValidator_ShouldFail_WhenManufacturerIsEmpty()
    {
        using var context = TestContextFactory.CreateContext();
        var validator = new CreateProductCommandValidator(context);

        var result = await validator.ValidateAsync(new CreateProductCommand(new ProductCreateDto
        {
            Name = "Dell XPS 15",
            Manufacturer = "",
        }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Product.Manufacturer");
    }

    [Fact]
    public async Task CreateProductCommandValidator_ShouldPass_WhenManufacturerIsProvided()
    {
        using var context = TestContextFactory.CreateContext();
        var validator = new CreateProductCommandValidator(context);

        var result = await validator.ValidateAsync(new CreateProductCommand(new ProductCreateDto
        {
            Name = "Dell XPS 15",
            Manufacturer = "Dell",
        }));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateProductCommandValidator_ShouldFail_WhenManufacturerIsEmpty()
    {
        using var context = TestContextFactory.CreateContext();
        var created = await CreateProduct(context);
        var validator = new UpdateProductCommandValidator(context);

        var result = await validator.ValidateAsync(new UpdateProductCommand(new ProductUpdateDto
        {
            Id = created.Id,
            Name = created.Name,
            Manufacturer = "   ",
        }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Product.Manufacturer");
    }

    private static async Task<ProductDto> CreateProduct(TestApplicationDbContext context, string name = "Dell XPS 15")
    {
        var handler = new CreateProductCommandHandler(context);
        return await handler.Handle(
            new CreateProductCommand(new ProductCreateDto { Name = name }),
            CancellationToken.None);
    }

    [Fact]
    public async Task UploadImageHandler_ShouldAttachImageUrl_ToExistingProduct()
    {
        using var context = TestContextFactory.CreateContext();
        var created = await CreateProduct(context);

        var uploadHandler = new UploadProductImageCommandHandler(context);
        var result = await uploadHandler.Handle(
            new UploadProductImageCommand(created.Id, "/uploads/products/test.png"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("/uploads/products/test.png", result!.ImageUrl);
    }

    [Fact]
    public async Task UploadImageHandler_ShouldReturnNull_WhenProductDoesNotExist()
    {
        using var context = TestContextFactory.CreateContext();
        var uploadHandler = new UploadProductImageCommandHandler(context);

        var result = await uploadHandler.Handle(
            new UploadProductImageCommand(999, "/uploads/products/test.png"),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductById_ShouldIncludeImageUrl_AfterUpload()
    {
        using var context = TestContextFactory.CreateContext();
        var created = await CreateProduct(context);
        var uploadHandler = new UploadProductImageCommandHandler(context);
        await uploadHandler.Handle(new UploadProductImageCommand(created.Id, "/uploads/products/test.png"), CancellationToken.None);

        var queryHandler = new GetProductByIdQueryHandler(context);
        var result = await queryHandler.Handle(new GetProductByIdQuery(created.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("/uploads/products/test.png", result!.ImageUrl);
    }

    [Fact]
    public async Task DeleteProductHandler_ShouldSoftDelete_AndExcludeFromSubsequentQueries()
    {
        using var context = TestContextFactory.CreateContext();
        var created = await CreateProduct(context);

        var deleteHandler = new DeleteProductCommandHandler(context);
        var deleted = await deleteHandler.Handle(new DeleteProductCommand(created.Id), CancellationToken.None);
        Assert.True(deleted);

        // The row must still physically exist (soft delete via AppDbContext.SaveChangesAsync's
        // Deleted -> Modified/IsDeleted interception), not be hard-removed — otherwise deleting a
        // product still referenced by an Asset (Restrict FK) would throw instead of succeeding.
        var stillInTable = context.Products.IgnoreQueryFilters().Any(p => p.Id == created.Id);
        Assert.True(stillInTable);

        var queryHandler = new GetProductByIdQueryHandler(context);
        var result = await queryHandler.Handle(new GetProductByIdQuery(created.Id), CancellationToken.None);
        Assert.Null(result);

        var listHandler = new GetProductsQueryHandler(context);
        var list = await listHandler.Handle(new GetProductsQuery(), CancellationToken.None);
        Assert.DoesNotContain(list, p => p.Id == created.Id);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnCreatedProduct_WithNullImageUrl_ByDefault()
    {
        using var context = TestContextFactory.CreateContext();
        await CreateProduct(context, "HP LaserJet Pro");

        var listHandler = new GetProductsQueryHandler(context);
        var list = await listHandler.Handle(new GetProductsQuery(), CancellationToken.None);

        var product = Assert.Single(list);
        Assert.Equal("HP LaserJet Pro", product.Name);
        Assert.Null(product.ImageUrl);
    }
}
