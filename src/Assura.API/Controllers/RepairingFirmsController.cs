using Assura.Application.Features.RepairingFirms.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

public class RepairingFirmsController : BaseApiController
{
    private readonly IMediator _mediator;

    public RepairingFirmsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RepairingFirmDto>>> GetRepairingFirms()
    {
        return await _mediator.Send(new GetRepairingFirmsQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateRepairingFirm([FromBody] CreateRepairingFirmCommand command)
    {
        return await _mediator.Send(command);
    }
}
