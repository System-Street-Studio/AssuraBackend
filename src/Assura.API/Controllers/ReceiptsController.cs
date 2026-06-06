using MediatR;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.Receipts.Queries.GetAll;
using Assura.Application.Features.Receipts.Commands.Create;
using Assura.Application.Features.Receipts.DTOs;
using Assura.Infrastructure.Persistence;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ReceiptsController(IMediator mediator, AppDbContext context, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReceiptDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllReceiptsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ReceiptDto>> Create([FromBody] CreateReceiptCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPost("{id}/upload")]
    public async Task<ActionResult<ReceiptDto>> UploadFile(string id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var receipt = await _context.Receipts.FirstOrDefaultAsync(r => r.Id.ToString() == id);
        if (receipt == null)
            return NotFound($"Receipt '{id}' not found.");

        // Ensure uploads directory exists
        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "receipts");
        Directory.CreateDirectory(uploadsDir);

        // Save file with unique name
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update receipt
        receipt.FileUrl = $"/uploads/receipts/{fileName}";
        receipt.Status = ReceiptStatus.Uploaded;
        await _context.SaveChangesAsync(default);

        return Ok(new ReceiptDto
        {
            Id = receipt.Id.ToString(),
            AssetName = receipt.AssetName,
            Division = receipt.Division,
            Date = receipt.Date.ToString("dd MMM yyyy"),
            Amount = receipt.Amount,
            Status = receipt.Status.ToString(),
            FileUrl = receipt.FileUrl
        });
    }
}
