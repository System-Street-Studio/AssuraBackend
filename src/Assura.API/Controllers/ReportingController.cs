using Assura.Application.Features.Reporting.DTOs;
using Assura.Application.Features.Reporting.Queries;
using Assura.Application.Features.Assets.Commands;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Auditor},{Roles.Admin}")]
public class ReportingController : BaseApiController
{
    private readonly IMediator _mediator;

    public ReportingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ReportingDashboardDto>> GetDashboard()
    {
        var result = await _mediator.Send(new GetReportingDashboardQuery());
        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ReportingAuditLogPageDto>> GetAuditLogs()
    {
        var result = await _mediator.Send(new GetReportingAuditLogsQuery());
        return Ok(result);
    }

    [HttpGet("assets")]
    public async Task<ActionResult<ReportingAssetsPageDto>> GetAssets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetReportingAssetsQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("reports")]
    public async Task<ActionResult<ReportingReportsPageDto>> GetReports()
    {
        var result = await _mediator.Send(new GetReportingReportsQuery());
        return Ok(result);
    }

    [HttpGet("reports/{type}/data")]
    public async Task<ActionResult<List<Dictionary<string, object>>>> GetReportData(
        string type, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null, 
        [FromQuery] int? divisionId = null)
    {
        var result = await _mediator.Send(new GetReportDataQuery(type, startDate, endDate, divisionId));
        return Ok(result);
    }

    [HttpPost("reports")]
    public async Task<ActionResult<string>> CreateReport(Assura.Application.Features.Reporting.Commands.CreateCustomReportCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }


    [HttpPost("assets/{id}/verify")]
    public async Task<ActionResult<bool>> VerifyAsset(int id)
    {
        var result = await _mediator.Send(new VerifyAssetCommand(id));
        return Ok(result);
    }
}
