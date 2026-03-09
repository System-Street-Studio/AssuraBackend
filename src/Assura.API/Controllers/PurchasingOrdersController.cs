using Assura.Application.PurchasingOrders.Commands;
using Assura.Application.PurchasingOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class PurchasingOrdersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<PurchasingOrdersController> _logger;

    public PurchasingOrdersController(IMediator mediator, ILogger<PurchasingOrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchasingOrderSummaryDto>>> GetPurchasingOrders()
    {
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: GetPurchasingOrders called");
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
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: Received request for supplier {SupplierName} with {Count} items", command.SupplierName, command.Items?.Count);
        try {
            var id = await _mediator.Send(command);
            _logger.LogInformation("[DEBUG] PurchasingOrdersController: Successfully created PO with ID {Id}", id);
            return Ok(id);
        } catch (Exception ex) {
            _logger.LogError(ex, "[DEBUG] PurchasingOrdersController: Error creating PO");
            throw;
        }
    }

    [HttpGet("pending-requests")]
    public async Task<ActionResult<List<AssetRequestDto>>> GetPendingRequests()
    {
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: GetPendingRequests called");
        return await _mediator.Send(new GetPendingAssetRequestsQuery());
    }
}
