namespace Assura.Application.Features.Assets.Queries;

/// <summary>
/// Lightweight DTO containing only the essential asset identifiers.
/// Used for dropdowns and summary lists where full asset details are not needed.
/// </summary>
public class AssetSummaryDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
}
