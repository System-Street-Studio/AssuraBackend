using System.Threading.Tasks;
using Assura.Application.Features.Depreciation.DTOs;
using Assura.Application.Features.Depreciation.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

[Authorize(Roles = $"{Roles.Superintendent},{Roles.Admin},{Roles.Accountant},{Roles.Auditor},{Roles.SystemAdmin}")]
public class DepreciationController : BaseApiController
{
    private readonly IMediator _mediator;

    public DepreciationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all assets with straight-line depreciation calculated based on category percentages.
    /// Can optionally simulate depreciation for a specific year end (e.g. 2026, 2027) or filter by category/division.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DepreciationSummaryDto>> GetDepreciation(
        [FromQuery] int? categoryId = null,
        [FromQuery] int? divisionId = null,
        [FromQuery] int? targetYear = null)
    {
        var result = await _mediator.Send(new GetAssetDepreciationQuery(categoryId, divisionId, targetYear));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the year-by-year straight-line depreciation schedule for a specific asset down to $0.00.
    /// </summary>
    [HttpGet("schedule/{assetId}")]
    public async Task<ActionResult<AssetDepreciationScheduleDto>> GetAssetSchedule(int assetId)
    {
        var result = await _mediator.Send(new GetAssetDepreciationScheduleQuery(assetId));
        if (result == null)
        {
            return NotFound(new { message = $"Asset with ID {assetId} not found." });
        }
        return Ok(result);
    }
}
