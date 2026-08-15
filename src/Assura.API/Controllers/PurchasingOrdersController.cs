using Assura.Application.PurchasingOrders.Commands;
using Assura.Application.PurchasingOrders.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize]
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
    public async Task<ActionResult<List<PurchasingOrderSummaryDto>>> GetPurchasingOrders([FromQuery] bool unregisteredOnly = false)
    {
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: GetPurchasingOrders called (unregisteredOnly={UnregisteredOnly})", unregisteredOnly);
        return await _mediator.Send(new GetPurchasingOrdersQuery(unregisteredOnly));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<bool>> UpdateStatus(int id, [FromBody] string? status)
    {
        var result = await _mediator.Send(new UpdatePurchasingOrderStatusCommand(id, string.IsNullOrWhiteSpace(status) ? "Registered" : status));
        if (!result) return NotFound();
        return Ok(true);
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<bool>> CompleteOrder(int id)
    {
        var result = await _mediator.Send(new UpdatePurchasingOrderStatusCommand(id, "Registered"));
        if (!result) return NotFound();
        return Ok(true);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchasingOrderDto>> GetPurchasingOrder(int id)
    {
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: GetPurchasingOrder called for ID {Id}", id);
        var result = await _mediator.Send(new GetPurchasingOrderByIdQuery(id));
        if (result == null) 
        {
            _logger.LogWarning("[DEBUG] PurchasingOrdersController: PO with ID {Id} not found", id);
            return NotFound();
        }
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: Successfully retrieved PO {OrderNumber}", result.OrderNumber);
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
    public async Task<ActionResult<List<Assura.Application.PurchasingOrders.Queries.AssetRequestDto>>> GetPendingRequests()
    {
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: GetPendingRequests called");
        return await _mediator.Send(new GetPendingAssetRequestsQuery());
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ProcurementStatsDto>> GetProcurementStats()
    {
        _logger.LogInformation("[DEBUG] PurchasingOrdersController: GetProcurementStats called");
        return await _mediator.Send(new GetProcurementStatsQuery());
    }
}
