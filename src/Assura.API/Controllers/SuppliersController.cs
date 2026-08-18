using Assura.Application.Features.Suppliers.Queries;
using Assura.Application.Features.Suppliers.Commands.CreateSupplier;
using Assura.Application.Features.Suppliers.Commands.UpdateSupplier;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = "Admin,Procurement,Storekeeper")]
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

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto?>> GetSupplierById(int id)
    {
        _logger.LogInformation("[DEBUG] SuppliersController: GetSupplierById called with id {Id}", id);
        return await _mediator.Send(new GetSupplierByIdQuery(id));
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateSupplier([FromBody] CreateSupplierCommand command)
    {
        _logger.LogInformation("[DEBUG] SuppliersController: CreateSupplier called with {@Command}", command);
        return await _mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateSupplier(int id, [FromBody] UpdateSupplierCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL path does not match ID in request body.");
        }

        _logger.LogInformation("[DEBUG] SuppliersController: UpdateSupplier called for id {Id} with {@Command}", id, command);
        var result = await _mediator.Send(command);
        if (!result)
        {
            return NotFound($"Supplier with ID {id} not found.");
        }

        return Ok(true);
    }
}
