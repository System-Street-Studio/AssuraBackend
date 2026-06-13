using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.AssetRequests.Commands;
using Assura.Domain.Entities;
using Assura.Application.Features.AssetRequests.Queries;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Assura.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssetRequestsController : ControllerBase
{
   private readonly IMediator _mediator;
   private readonly IWebHostEnvironment _env;

    public AssetRequestsController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateAssetRequestCommand command, [FromForm] List<IFormFile> files)
    {
        if (files != null && files.Count > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "requests");
            Directory.CreateDirectory(uploadsDir);
            
            var fileUrls = new List<string>();
            foreach(var file in files)
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                fileUrls.Add($"/uploads/requests/{fileName}");
            }
            command.AttachmentUrls = string.Join(",", fileUrls);
        }

        var id = await _mediator.Send(command);
        return Ok(id);
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<bool>> Approve(int id)
    {
        var result = await _mediator.Send(new ApproveAssetRequestCommand(id));
        return Ok(result);
    }

    [HttpPut("{id}/reject")] 
    public async Task<ActionResult<bool>> Reject(int id)
    {
        
        var result = await _mediator.Send(new RejectAssetRequestCommand(id));
        return Ok(result);
    }

    [HttpGet("employee/{employeeId}")] 
    public async Task<IActionResult> GetByEmployee(string employeeId)
    {
        try
        {
            var result = await _mediator.Send(new GetFilteredAssetRequestsQuery(null, null, employeeId, false));
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error in GetByEmployee: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, new { Message = "Server Error", Detail = ex.Message, Stack = ex.StackTrace });
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        Console.WriteLine($"[DEBUG] GetPending: userId={userId}, role={role}");

        // Admin/Procurement/Storekeeper see all pending requests
        if (role == "Admin" || role == "Procurement" || role == "Storekeeper")
        {
            return Ok(await _mediator.Send(new GetPendingRequestsQuery()));
        }
        
        // Safety: if we can't identify user, return empty
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(new List<object>());
        }

        // DivisionHead sees division requests, Employee sees only their own
        var isDivisionHead = role == "DivisionHead";
        var result = await _mediator.Send(new GetPendingRequestsQuery(userId, isDivisionHead));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? type = null, [FromQuery] bool isDivisionHead = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        // If Admin/Procurement/Storekeeper, they should see all requests
        if (role == "Admin" || role == "Procurement" || role == "Storekeeper")
        {
            var result = await _mediator.Send(new GetFilteredAssetRequestsQuery(status, type));
            return Ok(result);
        }

        // Safety: if we can't identify user, return empty
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(new List<object>());
        }
        
        var isHead = role == "DivisionHead";
        var filteredResult = await _mediator.Send(new GetFilteredAssetRequestsQuery(status, type, userId, isHead));
        return Ok(filteredResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetAssetRequestByIdQuery { Id = id });
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
}