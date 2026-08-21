using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Features.Transfers.Queries;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Assura.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

  

    //Creates a new transfer record linking a specific asset to an approved transfer request
   
   [HttpPost]
    [Authorize(Roles = "DivisionHead,Admin")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto dto)
    {
        try
        {
            // Validate request
            if (dto.AssetId <= 0 || dto.AssetRequestId <= 0)
                return BadRequest(new
                {
                    success = false,
                    error = "Invalid IDs",
                    message = "AssetId and AssetRequestId must be positive integers"
                });

            // UserId (who authorized the transfer) is taken from the caller's own JWT,
            // not the request body — the body value was previously trusted as-is, so any
            // caller could attribute a transfer to an arbitrary user id.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var callerId)) return Unauthorized();

            var command = new CreateTransferCommand
            {
                AssetId = dto.AssetId,
                AssetRequestId = dto.AssetRequestId,
                UserId = callerId
            };

            var transferId = await _mediator.Send(command);

            // Retrieve the created transfer with full details
            var transfer = await _mediator.Send(new GetTransferByIdQuery(transferId));

            return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id },
                new
                {
                    success = true,
                    message = "Transfer record created successfully",
                    data = transfer
                });
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($" Validation error creating transfer: {ex.Message}");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                details = ex.InnerException?.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($" Resource not found: {ex.Message}");
            return NotFound(new
            {
                success = false,
                error = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($" Authorization error creating transfer: {ex.Message}");
            return StatusCode(403, new
            {
                success = false,
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error creating transfer: {ex.Message}");
           Console.WriteLine($"FULL ERROR: {ex.ToString()}");

    return StatusCode(500, new
    {
        success = false,
        error = ex.Message,
        innerError = ex.InnerException?.Message
    });
        }
    }

   
    // Retrieves counts of transfers for the logged-in user's dashboard (Division Head
    // or Employee, depending on role — see GetTransferCountsQueryHandler). The caller
    // is always taken from the JWT, never trusted from the query string, so nobody can
    // view another user's (including another Division Head's) dashboard counts by
    // passing a different id.
    [HttpGet("counts")]
    public async Task<ActionResult<TransferCountsDto>> GetTransferCounts()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var query = new GetTransferCountsQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }


  
    // Retrieves the full transfer history for a specific asset — every Transfer row
    // it has ever been part of, oldest last. Open to any authenticated user, matching
    // AssetsController.GetAssetById's own lack of role restriction: anyone who can
    // already view an asset's details can see how it moved between employees.
    [HttpGet("asset/{assetId}")]
    public async Task<IActionResult> GetTransferHistoryForAsset(int assetId)
    {
        var result = await _mediator.Send(new GetAllTransfersQuery { AssetId = assetId });
        return Ok(result);
    }

    // Retrieves a specific transfer by ID

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetTransfer(int id)
    {
        try
        {
            var transfer = await _mediator.Send(new GetTransferByIdQuery(id));
            return Ok(new
            {
                success = true,
                data = transfer
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error retrieving transfer: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                error = "Internal server error",
                message = ex.Message
            });
        }
    }


    
    // Keep the feature branch's endpoint but map it to 'employee' so it doesn't conflict with 'GetAll'
    [HttpGet]
    public async Task<IActionResult> GetTransfers([FromQuery] string tab)
    {
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        int loginUserId = int.Parse(userIdClaim);

        var result = await _mediator.Send(new GetEmployeeTransferQuery(tab, loginUserId));
        return Ok(result);
    }


    // Accept transfer endpoint — only the asset's current holder may accept
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptTransfer(int id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim ?? "0");

            var result = await _mediator.Send(new AcceptTransferCommand(id, userId));

            if (result)
                return Ok(new { message = "Transfer accepted and is now pending your division head's approval." });

            return BadRequest("Failed to update transfer status");
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Reject transfer endpoint — only the asset's current holder may reject
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectTransfer(int id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim ?? "0");

            var result = await _mediator.Send(new RejectTransferCommand(id, userId));

            if (result)
                return Ok(new { message = "Transfer rejected successfully" });

            return BadRequest("Failed to update transfer status");
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Get division head transfers endpoint
    [HttpGet("division-head")]
    [Authorize(Roles = "DivisionHead")]
    public async Task<IActionResult> GetDivisionHeadTransfers([FromQuery] string tab)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        
        var userId = int.Parse(userIdClaim);
        var result = await _mediator.Send(new GetDivisionHeadTransferQuery(tab, userId));
        return Ok(result);
    }


    // Approve transfer by division head endpoint
    [HttpPost("{id}/approve-head")]
    [Authorize(Roles = "DivisionHead")]
    public async Task<IActionResult> ApproveByHead(int id)
    {
        try
        {
            var headIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int headId = int.Parse(headIdClaim ?? "0");
            
            var result = await _mediator.Send(new ApproveTransferByHeadCommand(id, headId));
            if (result)
                return Ok(new { message = "Transfer approved by division head" });
            return BadRequest("Failed to approve transfer");
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

 
    // Cancel transfer by division head endpoint
    [HttpPost("{id}/cancel-head")]
    [Authorize(Roles = "DivisionHead")]
    public async Task<IActionResult> CancelByHead(int id)
    {
        try
        {
            var headIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int headId = int.Parse(headIdClaim ?? "0");

            var result = await _mediator.Send(new CancelTransferByHeadCommand(id, headId));
            if (result)
                return Ok(new { success = true, message = "Transfer cancelled by division head" });
                
            return BadRequest(new { success = false, message = "Failed to cancel transfer. The transfer status may not allow cancellation at this stage." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // Confirm transfer by division head endpoint
    [HttpPost("{id}/confirm-head")]
    [Authorize(Roles = "DivisionHead")]
    public async Task<IActionResult> ConfirmByHead(int id)
    {
        try
        {
            var headIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int headId = int.Parse(headIdClaim ?? "0");

            var result = await _mediator.Send(new ConfirmTransferByHeadCommand(id, headId));
            if (result)
                return Ok(new { message = "Transfer confirmed by division head" });
            return BadRequest("Failed to confirm transfer");
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/reject-head")]
    [Authorize(Roles = "DivisionHead")]
    public async Task<IActionResult> RejectByHead(int id, [FromBody] RejectHeadDto dto)
    {
        try
        {
            // Was reading a non-existent "UserId" claim (the JWT only issues the
            // standard NameIdentifier claim — see JwtTokenGenerator), so headId was
            // always 0 here regardless of who called this endpoint. Matches the
            // claim lookup used by the other three by-head actions above.
            var headIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int headId = int.Parse(headIdClaim ?? "0");

            var command = new RejectTransferByHeadCommand(id, headId, dto.Reason);
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "Rejected by head successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> ReturnActiveTransfer(int id)
    {
        try
        {
            var callerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(callerIdClaim, out var callerId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var isDivisionHead = User.IsInRole("DivisionHead");

            var result = await _mediator.Send(new ReturnActiveTransferCommand(id, callerId, isAdmin, isDivisionHead));

            if (!result)
            {
                return NotFound(new { success = false, message = $"Transfer record with ID {id} not found, already completed, or not active." });
            }
            return Ok(new { success = true, message = "Asset returned successfully. Transfer marked as Completed and Asset status updated to In Use." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during asset return: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An error occurred while returning the asset.", error = ex.Message });
        }
    }

        public class RejectHeadDto 
        { 
            public string Reason { get; set; } = string.Empty; 
        }
}