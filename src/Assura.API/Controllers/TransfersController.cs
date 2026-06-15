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

  
    /// POST /api/transfers
    /// Creates a new transfer record linking a specific asset to an approved transfer request
   
   [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto dto)
    {
        try
        {
            // Validate request
            if (dto.AssetId <= 0 || dto.AssetRequestId <= 0 || dto.UserId <= 0)
                return BadRequest(new
                {
                    success = false,
                    error = "Invalid IDs",
                    message = "AssetId, AssetRequestId, and UserId must be positive integers"
                });

            var command = new CreateTransferCommand
            {
                AssetId = dto.AssetId,
                AssetRequestId = dto.AssetRequestId,
                UserId = dto.UserId
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

    // GET /api/transfers/counts
    // Retrieves counts of transfers for the logged-in user's division head dashboard
    /// GET /api/transfers/counts?userId=77
    [HttpGet("counts")]
    public async Task<ActionResult<TransferCountsDto>> GetTransferCounts([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("Invalid User ID");

        var query = new GetTransferCountsQuery(userId);
        var result = await _mediator.Send(query); 
        return Ok(result);
    }


    /// GET /api/transfers/{id}
    /// Retrieves a specific transfer by ID
   
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

    // GET /api/transfers?tab={tab} or /api/transfers?divisionId={divisionId}
    // Retrieves transfers for the logged-in user based on the specified tab, or transfers by division
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page = 1, [FromQuery] int? pageSize = 50, [FromQuery] int? divisionId = null, [FromQuery] string? status = null, [FromQuery] int? employeeId = null)
    {
        Console.WriteLine(" === TRANSFER API ENDPOINT CALLED ===");
        Console.WriteLine($" GET /api/transfers called at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($" API received query parameters:");
        Console.WriteLine($"  divisionId: {divisionId}");
        Console.WriteLine($"  status: {status}");
        Console.WriteLine($"  employeeId: {employeeId}");

        try
        {
            var query = new GetAllTransfersQuery
            {
                DivisionId = divisionId,
                Status = status,
                EmployeeId = employeeId
            };

            var transfers = await _mediator.Send(query);
            
            return Ok(new 
            { 
                success = true,
                data = transfers
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error retrieving transfers: {ex.Message}" 
            });
        }
    }

    [HttpGet("incoming")]
    public async Task<IActionResult> GetIncomingTransfers([FromQuery] int? page = 1, [FromQuery] int? pageSize = 50, [FromQuery] int? userId = null)
    {
        Console.WriteLine("=== GET INCOMING TRANSFERS ===");
        Console.WriteLine($"GET /api/transfers/incoming called at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"API received query parameters:");
       
        Console.WriteLine($"  userId: {userId}");
        Console.WriteLine($"🔍 Filtering for status = 1 (PendingOwnerApproval)");

        try
        {
            var query = new GetAllTransfersQuery
            {
                
                Status = "1" // Status = 1 for PendingOwnerApproval
            };

            var transfers = await _mediator.Send(query);
            
            Console.WriteLine($"📊 Found {transfers.Count} incoming transfers with status = 1");
            
            if (transfers.Count > 0)
            {
                Console.WriteLine("📋 Incoming transfers:");
                for (int i = 0; i < Math.Min(5, transfers.Count); i++)
                {
                    var t = transfers[i];
                    Console.WriteLine($"  {i+1}. ID:{t.Id} | {t.TransferNumber} | Asset:{t.AssetId} | From:{t.FromDivisionName} | To:{t.ToDivisionName} | Target:{t.TargetUserName} | Status:{t.Status}");
                }
            }
            else
            {
                Console.WriteLine("❌ No incoming transfers found with status = 1");
            }
            
            return Ok(new 
            { 
                success = true,
                message = "Incoming transfers retrieved successfully",
                data = transfers
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error retrieving incoming transfers: {ex.Message}");
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error retrieving incoming transfers: {ex.Message}" 
            });
        }
    }

    [HttpGet("user-transfers")]
    public async Task<IActionResult> GetUserTransfers([FromQuery] int? currentHolderId = null)
    {
        Console.WriteLine("=== GET USER TRANSFERS ===");
        Console.WriteLine($"GET /api/transfers/user-transfers called at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"API received query parameters:");
        Console.WriteLine($"  currentHolderId: {currentHolderId}");
        Console.WriteLine($"🔍 Filtering transfers for currentHolderId: {currentHolderId}");

        try
        {
            var query = new GetAllTransfersQuery
            {
                CurrentHolderId = currentHolderId
            };

            var transfers = await _mediator.Send(query);
            
            Console.WriteLine($"📊 Found {transfers.Count} transfers for currentHolderId: {currentHolderId}");
            
            if (transfers.Count > 0)
            {
                Console.WriteLine("📋 User transfers:");
                for (int i = 0; i < Math.Min(5, transfers.Count); i++)
                {
                    var t = transfers[i];
                    Console.WriteLine($"  {i+1}. ID:{t.Id} | {t.TransferNumber} | Asset:{t.AssetId} | From:{t.FromDivisionName} | To:{t.ToDivisionName} | Status:{t.Status} | CurrentHolder:{t.CurrentHolderName}");
                }
            }
            else
            {
                Console.WriteLine("❌ No transfers found for this user");
            }
            
            return Ok(new 
            { 
                success = true,
                message = "User transfers retrieved successfully",
                data = transfers
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error retrieving user transfers: {ex.Message}");
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error retrieving user transfers: {ex.Message}" 
            });
        }
    }

    // Keep the feature branch's endpoint but map it to 'employee' so it doesn't conflict with 'GetAll'
    [HttpGet("employee")]
    public async Task<IActionResult> GetTransfers([FromQuery] string tab)
    {
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        int loginUserId = int.Parse(userIdClaim);

        var result = await _mediator.Send(new GetEmployeeTransferQuery(tab, loginUserId));
        return Ok(result);
    }

    // Accept transfer endpoint
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptTransfer(int id)
    {
        try
        {
            
            var result = await _mediator.Send(new AcceptTransferCommand(id));
            
            if (result)
                return Ok(new { message = "Transfer status updated to Waiting for Final Confirmation" });
            
            return BadRequest("Failed to update transfer status");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Reject transfer endpoint
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectTransfer(int id)
    {
        try
        {
            
            var result = await _mediator.Send(new RejectTransferCommand(id));
            
            if (result)
                return Ok(new { message = "Transfer rejected successfully" });
            
            return BadRequest("Failed to update transfer status");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Get division head transfers endpoint
    [HttpGet("division-head")]
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
    public async Task<IActionResult> ApproveByHead(int id)
    {
        try
        {
            var result = await _mediator.Send(new ApproveTransferByHeadCommand(id));
            if (result)
                return Ok(new { message = "Transfer approved by division head" });
            return BadRequest("Failed to approve transfer");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Cancel transfer by division head endpoint
    [HttpPost("{id}/cancel-head")]
    public async Task<IActionResult> CancelByHead(int id)
    {
        try
        {
            var result = await _mediator.Send(new CancelTransferByHeadCommand(id));
            if (result)
                return Ok(new { message = "Transfer cancelled by division head" });
            return BadRequest("Failed to cancel transfer");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Confirm transfer by division head endpoint
    [HttpPost("{id}/confirm-head")]
    public async Task<IActionResult> ConfirmByHead(int id)
    {
        try
        {
            var result = await _mediator.Send(new ConfirmTransferByHeadCommand(id));
            if (result)
                return Ok(new { message = "Transfer confirmed by division head" });
            return BadRequest("Failed to confirm transfer");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/reject-head")]
    public async Task<IActionResult> RejectByHead(int id, [FromBody] RejectHeadDto dto)
    {
        try
        {
            var headIdClaim = User.FindFirst("UserId")?.Value ?? "0";
            int headId = int.Parse(headIdClaim);

            var command = new RejectTransferByHeadCommand(id, headId, dto.Reason);
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "Rejected by head successfully" });
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
            var result = await _mediator.Send(new ReturnActiveTransferCommand(id));
            
            if (!result)
            {
                return NotFound(new { success = false, message = $"Transfer record with ID {id} not found, already completed, or not active." });
            }
            return Ok(new { success = true, message = "Asset returned successfully. Transfer marked as Completed and Asset status updated to In Use." });
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