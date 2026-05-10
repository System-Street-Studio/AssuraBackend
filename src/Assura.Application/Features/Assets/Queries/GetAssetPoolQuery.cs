using MediatR;
using Assura.Application.Features.Assets.DTOs;

namespace Assura.Application.Features.Assets.Queries;

public record GetAssetPoolQuery(
    string? Search = null,
    string? Category = null,
    string? Division = null,
    int? EmployeeId = null,
    string? SpecName = null,
    string? SpecValue = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<AssetPoolFilterResult>;
