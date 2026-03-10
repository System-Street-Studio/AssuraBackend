using Assura.Application.Features.Suppliers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class SuppliersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(IMediator mediator, ILogger<SuppliersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetSuppliers()
    {
        _logger.LogInformation("[DEBUG] SuppliersController: GetSuppliers called");
        return await _mediator.Send(new GetSuppliersQuery());
    }

    // You can add more endpoints here like GetSupplierById, CreateSupplier, etc.
}
