using Assura.Application.DTOs;
using Assura.Application.Features.Assets.Commands;
using Assura.Application.Features.Assets.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assura.API.Controllers;

 [Authorize] 
public class AssetsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Retrieves a list of assets with optional filters for ownership a
    [HttpGet]
    public async Task<ActionResult<List<AssetDto>>> GetAssets([FromQuery] bool onlyMine = false)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst("sub")?.Value;

        int? userId = int.TryParse(userIdStr, out var id) ? id : null;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (onlyMine && userId.HasValue)
        {
            return await _mediator.Send(new GetAssetsQuery(userId));
        }
        
        if (role == Roles.Admin || role == Roles.Storekeeper || role == Roles.Procurement || role == Roles.Auditor || role == Roles.Superintendent || role == Roles.Accountant)
        {
            return await _mediator.Send(new GetAssetsQuery());
        }

        
        if (role == Roles.DivisionHead && userId.HasValue)
        {
            
           
            
            return await _mediator.Send(new GetAssetsQuery(null, userId, role));
        }

       
        return await _mediator.Send(new GetAssetsQuery(userId));
    }
    
    

    [HttpGet("available-for-checkout")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<List<AvailableCheckoutAssetDto>>> GetAvailableForCheckout()
    {
        return await _mediator.Send(new GetAvailableAssetsForCheckoutQuery());
    }

    [HttpGet("checkout-records")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper},{Roles.Auditor}")]
    public async Task<ActionResult<List<CheckoutRecordDto>>> GetCheckoutRecords()
    {
        return await _mediator.Send(new GetCheckoutRecordsQuery());
    }

    [HttpPost("{id}/checkout")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<CheckoutRecordDto>> CheckoutAsset(int id, [FromBody] CheckoutRequest request)
    {
        if (id <= 0 || request.AssigneeUserId <= 0)
        {
            return BadRequest("Invalid asset or assignee.");
        }

        var actorName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Storekeeper";

        var result = await _mediator.Send(new CheckoutAssetCommand(
            id,
            request.AssigneeUserId,
            request.DueDate,
            request.Notes,
            actorName));

        return Ok(result);
    }

    // Retrieves a specific asset by its ID.
     [HttpGet("{id}")]
    public async Task<ActionResult<Assura.Application.DTOs.AssetDto>> GetAsset(int id)
    {
        var result = await _mediator.Send(new GetAssetByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<Assura.Application.DTOs.AssetDto>> CreateAsset(AssetCreateDto asset)
    {
        var result = await _mediator.Send(new CreateAssetCommand(asset));
        return CreatedAtAction(nameof(GetAsset), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<Assura.Application.DTOs.AssetDto>> UpdateAsset(int id, AssetUpdateDto asset)
    {
        if (id != asset.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateAssetCommand(asset));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult> PatchAssetStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateAssetStatusCommand(id, request.Status));
        if (!result) return NotFound();
        return NoContent();
    }

    public class UpdateStatusRequest
    {
        public Assura.Domain.Enums.AssetStatus Status { get; set; }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult> DeleteAsset(int id)
    {
        var result = await _mediator.Send(new DeleteAssetCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/checkin")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Storekeeper}")]
    public async Task<ActionResult<Assura.Application.DTOs.AssetDto>> CheckinAsset(int id, [FromBody] CheckinRequest request)
    {
        var actorName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Storekeeper";
        var result = await _mediator.Send(new CheckinAssetCommand(
            id,
            request.Condition,
            request.Notes,
            actorName,
            request.DamageSeverity,
            request.RepairNeeded,
            request.Acknowledged,
            request.EvidenceFileName));
        if (result == null) return NotFound();
        return Ok(result);
    }

    public class CheckoutRequest
    {
        public int AssigneeUserId { get; set; }
        public DateOnly DueDate { get; set; }
        public string? Notes { get; set; }
    }

    public class CheckinRequest
    {
        public string Condition { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? DamageSeverity { get; set; }
        public bool RepairNeeded { get; set; }
        public bool Acknowledged { get; set; }
        public string? EvidenceFileName { get; set; }
    }



}
