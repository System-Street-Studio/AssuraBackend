using Assura.Application.Features.Reporting.DTOs;
using Assura.Application.Features.Reporting.Queries;
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
    public async Task<ActionResult<ReportingAssetsPageDto>> GetAssets()
    {
        var result = await _mediator.Send(new GetReportingAssetsQuery());
        return Ok(result);
    }

    [HttpGet("reports")]
    public async Task<ActionResult<ReportingReportsPageDto>> GetReports()
    {
        var result = await _mediator.Send(new GetReportingReportsQuery());
        return Ok(result);
    }
}
