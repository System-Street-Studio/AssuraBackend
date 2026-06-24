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

    // Creates a new asset request.
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateAssetRequestApiInput input)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(input.EmployeeId))
                return BadRequest(new { message = "EmployeeId is required" });
            if (string.IsNullOrWhiteSpace(input.SubmittedBy))
                return BadRequest(new { message = "SubmittedBy is required" });
            if (string.IsNullOrWhiteSpace(input.AssetName))
                return BadRequest(new { message = "AssetName is required" });
            if (string.IsNullOrWhiteSpace(input.Priority))
                return BadRequest(new { message = "Priority is required" });
            if (string.IsNullOrWhiteSpace(input.RequestType))
                return BadRequest(new { message = "RequestType is required" });

            var savedAttachments = new List<AttachmentUploadModel>();

            
            if (input.Files != null && input.Files.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var file in input.Files)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        savedAttachments.Add(new AttachmentUploadModel
                        {
                            FileName = file.FileName,
                            FileUrl = $"/uploads/{uniqueFileName}",
                            FileSize = file.Length,
                            FileType = file.ContentType
                        });
                    }
                }
            }

           
            var command = new CreateAssetRequestCommand
            {
                EmployeeId = input.EmployeeId,
                SubmittedBy = input.SubmittedBy,
                AssetCategory = input.AssetCategory ?? string.Empty,
                AssetName = input.AssetName,
                Description = input.Description ?? string.Empty,
                Reason = input.Reason ?? string.Empty,
                Quantity = input.Quantity,
                Priority = input.Priority,
                RequestType = input.RequestType,
                SubmittedDate = input.SubmittedDate == default ? DateTime.Now : input.SubmittedDate,
                AssetId = input.AssetId,
                UploadedAttachments = savedAttachments
            };

            var id = await _mediator.Send(command);
            return Ok(new { id, message = "Asset request created successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the asset request", error = ex.Message });
        }
    }


    
    public class CreateAssetRequestApiInput
    {
        public string? EmployeeId { get; set; }
        public string? SubmittedBy { get; set; }
        public string? AssetCategory { get; set; }
        public string? AssetName { get; set; }
        public string? Description { get; set; }
        public string? Reason { get; set; }
        public int Quantity { get; set; }
        public string? Priority { get; set; }
        public string? RequestType { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int? AssetId { get; set; }
        public List<IFormFile>? Files { get; set; }
    }


    // Approves an asset request by its ID.
    [HttpPut("{id}/approve")]
    public async Task<ActionResult<bool>> Approve(int id)
    {
        var result = await _mediator.Send(new ApproveAssetRequestCommand(id));
        return Ok(result);
    }

    // Rejects an asset request by its ID.
    [HttpPut("{id}/reject")] 
    public async Task<ActionResult<bool>> Reject(int id)
    {
        
        var result = await _mediator.Send(new RejectAssetRequestCommand(id));
        return Ok(result);
    }

    // Retrieves all asset requests made by a specific employee.
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

    // Retrieves all asset requests that are pending approval for a specific division head.
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;


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

    // Retrieves all asset requests with optional filters for status and type.
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

        // Retrieves all approved asset transfer requests for a specific division.
    [HttpGet("approved-transfers")]
    public async Task<IActionResult> GetApprovedTransfers([FromQuery] int? headId = null)
    {
        var result = await _mediator.Send(new GetApprovedTransfersQuery(headId));
        return Ok(result);
    }

    // Retrieves a specific asset request by its ID.
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