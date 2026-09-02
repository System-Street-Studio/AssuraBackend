using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assura.Application.Common.Interfaces;
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
    private readonly IFileStorageService _fileStorage;

    public ReceiptsController(IMediator mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
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

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";

        string virtualPath;
        await using (var stream = file.OpenReadStream())
        {
            virtualPath = await _fileStorage.SaveAsync(stream, "receipts", fileName, file.ContentType);
        }

        var result = await _mediator.Send(new UploadReceiptFileCommand(id, virtualPath));
        if (result == null)
            return NotFound($"Receipt '{id}' not found.");

        return Ok(result);
    }
}
