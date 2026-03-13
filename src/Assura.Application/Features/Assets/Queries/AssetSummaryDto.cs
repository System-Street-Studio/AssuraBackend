namespace Assura.Application.Features.Assets.Queries;

public class AssetSummaryDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
}
