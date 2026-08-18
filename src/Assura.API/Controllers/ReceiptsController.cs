using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Features.Receipts.Queries.GetAll;
using Assura.Application.Features.Receipts.Commands.Create;
using Assura.Application.Features.Receipts.Commands.UploadFile;
using Assura.Application.Features.Receipts.DTOs;
using Assura.Domain.Constants;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Accountant},{Roles.Admin}")]
public class ReceiptsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public ReceiptsController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
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

        var result = await _mediator.Send(new UploadReceiptFileCommand(id, $"/uploads/receipts/{fileName}"));
        if (result == null)
            return NotFound($"Receipt '{id}' not found.");

        return Ok(result);
    }
}
