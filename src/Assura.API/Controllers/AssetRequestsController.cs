using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.AssetRequests.Commands;
using Assura.Domain.Entities;
using Assura.Application.Features.AssetRequests.Queries;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Assura.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssetRequestsController : ControllerBase
{
   private readonly IMediator _mediator;
   private readonly IFileStorageService _fileStorage;

    public AssetRequestsController(IMediator mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
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

            // EmployeeId/SubmittedBy are always overridden below from the authenticated
            // user's identity, never trusted from the client — otherwise any caller could
            // submit a request impersonating a different employee.
            var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(callerId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(input.AssetName))
                return BadRequest(new { message = "AssetName is required" });
            if (string.IsNullOrWhiteSpace(input.Priority))
                return BadRequest(new { message = "Priority is required" });
            if (string.IsNullOrWhiteSpace(input.RequestType))
                return BadRequest(new { message = "RequestType is required" });

            var savedAttachments = new List<AttachmentUploadModel>();

            
            if (input.Files != null && input.Files.Count > 0)
            {
                foreach (var file in input.Files)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

                        string virtualPath;
                        await using (var stream = file.OpenReadStream())
                        {
                            virtualPath = await _fileStorage.SaveAsync(stream, string.Empty, uniqueFileName, file.ContentType);
                        }

                        savedAttachments.Add(new AttachmentUploadModel
                        {
                            FileName = file.FileName,
                            FileUrl = virtualPath,
                            FileSize = file.Length,
                            FileType = file.ContentType
                        });
                    }
                }
            }

           
            var command = new CreateAssetRequestCommand
            {
                EmployeeId = callerId,
                SubmittedBy = input.SubmittedBy ?? string.Empty,
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
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the asset request", error = ex.Message });
        }
    }


    
    public class RejectAssetRequestApiInput
    {
        public string? Reason { get; set; }
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


    // Cancels a pending asset request. The requester can withdraw their own request;
    // staff roles can also cancel on behalf of an employee.
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var isPrivileged = role == "Admin" || role == "Storekeeper" || role == "Procurement" || role == "DivisionHead";

        var result = await _mediator.Send(new CancelAssetRequestCommand(id, userId, isPrivileged));

        return result switch
        {
            CancelAssetRequestResult.Success => NoContent(),
            CancelAssetRequestResult.NotFound => NotFound(),
            CancelAssetRequestResult.Forbidden => Forbid(),
            CancelAssetRequestResult.InvalidStatus => Conflict(new { message = "Only pending requests can be cancelled." }),
            _ => StatusCode(500)
        };
    }

    // Approves an asset request by its ID. Only the Division Head of the requesting
    // division (or Admin) may approve — enforced both by role and, in the handler,
    // by matching the caller's division against the request's division.
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "DivisionHead,Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var (userId, role) = GetCallerIdentity();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new ApproveAssetRequestCommand(id, userId.Value, role == "Admin"));
        return result switch
        {
            ApproveAssetRequestResult.Success => Ok(true),
            ApproveAssetRequestResult.NotFound => NotFound(),
            ApproveAssetRequestResult.Forbidden => Forbid(),
            ApproveAssetRequestResult.InvalidStatus => Conflict(new { message = "Only pending requests can be approved." }),
            _ => StatusCode(500)
        };
    }

    // Rejects an asset request by its ID. Same division-scoped authorization as Approve.
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "DivisionHead,Admin")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectAssetRequestApiInput? input = null)
    {
        var (userId, role) = GetCallerIdentity();
        if (userId == null) return Unauthorized();

        var result = await _mediator.Send(new RejectAssetRequestCommand(id, userId.Value, role == "Admin", input?.Reason));
        return result switch
        {
            RejectAssetRequestResult.Success => Ok(true),
            RejectAssetRequestResult.NotFound => NotFound(),
            RejectAssetRequestResult.Forbidden => Forbid(),
            RejectAssetRequestResult.InvalidStatus => Conflict(new { message = "Only pending requests can be rejected." }),
            _ => StatusCode(500)
        };
    }

    private (int? UserId, string? Role) GetCallerIdentity()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return int.TryParse(userIdStr, out var userId) ? (userId, role) : (null, role);
    }

    // Retrieves all asset requests made by a specific employee.
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(string employeeId)
    {
        try
        {
            var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

            // Employees may only ever list their own requests; only staff roles can look
            // up another employee's history by id (prevents IDOR via a guessed id).
            var isStaffRole = callerRole == "Admin" || callerRole == "Procurement"
                || callerRole == "Storekeeper" || callerRole == "DivisionHead";

            if (!isStaffRole && (string.IsNullOrEmpty(callerId) || callerId != employeeId))
            {
                return Forbid();
            }

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

        // Retrieves approved asset transfer requests, scoped to the caller's own
        // division for Division Head (Admin sees every division). The caller's id is
        // always taken from the JWT — never trusted from the query string — so a
        // Division Head can't see another division's approved transfers by passing a
        // different headId (or none at all).
    [HttpGet("approved-transfers")]
    [Authorize(Roles = "DivisionHead,Admin")]
    public async Task<IActionResult> GetApprovedTransfers()
    {
        var (userId, role) = GetCallerIdentity();
        if (userId == null) return Unauthorized();

        var headId = role == "Admin" ? (int?)null : userId;
        var result = await _mediator.Send(new GetApprovedTransfersQuery(headId));
        return Ok(result);
    }

    // Retrieves a specific asset request by its ID.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        int? userId = int.TryParse(userIdStr, out var uid) ? uid : null;

        var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
        Assura.Domain.Enums.UserRole? role = Enum.TryParse<Assura.Domain.Enums.UserRole>(roleStr, true, out var r) ? r : null;

        var result = await _mediator.Send(new GetAssetRequestByIdQuery { Id = id, UserId = userId, Role = role });

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
    
}