using Assura.Application.PurchasingOrders.Commands;
using Assura.Application.PurchasingOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class PurchasingOrdersController : BaseApiController
{
    private readonly IMediator _mediator;

    public PurchasingOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchasingOrderSummaryDto>>> GetPurchasingOrders()
    {
        return await _mediator.Send(new GetPurchasingOrdersQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchasingOrderDto>> GetPurchasingOrder(int id)
    {
        var result = await _mediator.Send(new GetPurchasingOrderByIdQuery(id));
        if (result == null) return NotFound();
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreatePurchasingOrder(CreatePurchasingOrderCommand command)
    {
        Console.WriteLine($"[DEBUG] PurchasingOrdersController: Received request for supplier {command.SupplierName} with {command.Items?.Count} items");
        try {
            var id = await _mediator.Send(command);
            Console.WriteLine($"[DEBUG] PurchasingOrdersController: Successfully created PO with ID {id}");
            return Ok(id);
        } catch (Exception ex) {
            Console.WriteLine($"[DEBUG] PurchasingOrdersController: Error creating PO: {ex.Message}");
            throw;
        }
    }

    [HttpGet("pending-requests")]
    public async Task<ActionResult<List<AssetRequestDto>>> GetPendingRequests()
    {
        return await _mediator.Send(new GetPendingAssetRequestsQuery());
    }
}
