using Microsoft.AspNetCore.Mvc;
using MediatR;
using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Features.Transfers.Queries;
using Assura.Application.Features.Transfers.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Assura.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

   // Create transfer endpoint
    [HttpPost]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto dto)
    {
        try
        {
        
            var command = new CreateTransferCommand
            {
                AssetId = dto.AssetId,
                AssetRequestId = dto.AssetRequestId
            };

            var result = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                message = "Transfer created successfully",
                transferId = result
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating transfer: {ex.Message}");
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page = 1, [FromQuery] int? pageSize = 50, [FromQuery] int? divisionId = null, [FromQuery] string? status = null)
    {
        Console.WriteLine(" === TRANSFER API ENDPOINT CALLED ===");
        Console.WriteLine($" GET /api/transfers called at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($" API received query parameters:");
        Console.WriteLine($"  page: {page}");
        Console.WriteLine($"  pageSize: {pageSize}");
        Console.WriteLine($"  divisionId: {divisionId}");
        Console.WriteLine($"  status: {status}");

        try
        {
            var query = new GetAllTransfersQuery
            {
                Page = page,
                PageSize = pageSize,
                DivisionId = divisionId,
                Status = status
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        Console.WriteLine($"=== GET TRANSFER BY ID ===");
        Console.WriteLine($"GET /api/transfers/{id} called");
        
        try
        {
            var query = new GetAllTransfersQuery { AssetId = id };
            var transfers = await _mediator.Send(query);
            var transfer = transfers.FirstOrDefault(t => t.Id == id);

            if (transfer == null)
            {
                Console.WriteLine($"❌ Transfer with ID {id} not found");
                return NotFound(new 
                { 
                    success = false, 
                    message = "Transfer not found" 
                });
            }

            Console.WriteLine($"✅ Found transfer: {transfer.TransferNumber}");
            return Ok(new 
            { 
                success = true, 
                data = transfer 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error retrieving transfer: {ex.Message}");
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error retrieving transfer: {ex.Message}" 
            });
        }
    }

    [HttpGet("verify")]
    public async Task<IActionResult> VerifyTransfers()
    {
        Console.WriteLine("=== VERIFY TRANSFER TABLE ===");
        Console.WriteLine("GET /api/transfers/verify called");
        
        try
        {
            var query = new GetAllTransfersQuery { PageSize = 1000 };
            var transfers = await _mediator.Send(query);
            
            Console.WriteLine($"📊 Total transfers in database: {transfers.Count}");
            
            if (transfers.Count > 0)
            {
                Console.WriteLine("📋 Recent transfers:");
                for (int i = 0; i < Math.Min(5, transfers.Count); i++)
                {
                    var t = transfers[i];
                    Console.WriteLine($"  {i+1}. ID:{t.Id} | {t.TransferNumber} | Asset:{t.AssetId} | From:{t.FromDivisionName} | To:{t.ToDivisionName} | Status:{t.Status} | Created:{t.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                }
            }
            else
            {
                Console.WriteLine("❌ No transfers found in database!");
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
            Console.WriteLine($"❌ Error verifying transfers: {ex.Message}");
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error verifying transfers: {ex.Message}" 
            });
        }
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            // Implementation for approving transfer
            // This would update the transfer status to Approved
            return Ok(new 
            { 
                success = true, 
                message = "Transfer approved successfully" 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error approving transfer: {ex.Message}" 
            });
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            // Implementation for rejecting transfer
            // This would update the transfer status to Rejected
            return Ok(new 
            { 
                success = true, 
                message = "Transfer rejected successfully" 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                success = false, 
                message = $"Error rejecting transfer: {ex.Message}" 
            });
        }
    }
}