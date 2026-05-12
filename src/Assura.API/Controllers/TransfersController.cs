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
        innerError = ex.InnerException?.Message // Inner Exception එකත් බලන්න
    });
        }
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

    // Get transfers filtered by division
    [HttpGet("by-division")]
    public async Task<IActionResult> GetTransfersByDivision([FromQuery] int divisionId)
    {
        try
        {
            if (divisionId <= 0)
                return BadRequest(new { success = false, message = "Invalid division ID" });

            var result = await _mediator.Send(new GetAllTransfersQuery { DivisionId = divisionId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving transfers by division: {ex.Message}");
            return StatusCode(500, new 
            { 
                success = false, 
                message = "Error retrieving transfers",
                error = ex.Message 
            });
        }
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

   

    // Verify transfers endpoint
    [HttpGet("verify")]
    public async Task<IActionResult> VerifyTransfers()
    {
    
        Console.WriteLine("GET /api/transfers/verify called");
        
        try
        {
            var query = new GetAllTransfersQuery { };
            var transfers = await _mediator.Send(query);
            
            Console.WriteLine($" Total transfers in database: {transfers.Count}");
            
            if (transfers.Count > 0)
            {
                Console.WriteLine(" Recent transfers:");
                for (int i = 0; i < Math.Min(5, transfers.Count); i++)
                {
                    var t = transfers[i];
                    Console.WriteLine($"  {i+1}. ID:{t.Id} | {t.TransferNumber} | Asset:{t.AssetId} | From:{t.FromDivisionName} | To:{t.ToDivisionName} | Status:{t.Status} | Created:{t.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }
            }
            else
            {
                Console.WriteLine(" No transfers found in database!");
            }
            
            return Ok(new 
            { 
                success = true,
                message = "Transfer verification completed",
                data = new 
                { 
                    totalTransfers = transfers.Count,
                    recentTransfers = transfers.Take(5).ToList(),
                    databaseStatus = transfers.Count > 0 ? "Populated" : "Empty"
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error verifying transfers: {ex.Message}");
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error verifying transfers: {ex.Message}" 
            });
        }
    }

    
}