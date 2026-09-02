using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Application.Features.Products.Commands;
using Assura.Application.Features.Products.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Procurement},{Roles.Storekeeper},{Roles.Auditor}")]
public class ProductsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    public ProductsController(IMediator mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        return await _mediator.Send(new GetProductsQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductCreateDto product)
    {
        var result = await _mediator.Send(new CreateProductCommand(product));
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, ProductUpdateDto product)
    {
        if (id != product.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateProductCommand(product));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/upload")]
    public async Task<ActionResult<ProductDto>> UploadImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        if (file.Length > MaxImageSizeBytes)
            return BadRequest("Image must be 5MB or smaller.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return BadRequest("Only JPG, PNG, WEBP or GIF images are allowed.");

        var fileName = $"{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";

        string virtualPath;
        await using (var stream = file.OpenReadStream())
        {
            virtualPath = await _fileStorage.SaveAsync(stream, "products", fileName, file.ContentType);
        }

        var result = await _mediator.Send(new UploadProductImageCommand(id, virtualPath));
        if (result == null)
        {
            await _fileStorage.DeleteAsync(virtualPath);
            return NotFound();
        }

        return Ok(result);
    }
}
