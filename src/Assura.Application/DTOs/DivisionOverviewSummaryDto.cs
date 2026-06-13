namespace Assura.Application.DTOs;

public record DivisionOverviewSummaryDto(
    int AssetsCount,
    decimal AssetsPurchaseValue,
    int PendingRequestsCount,
    int TransferredAssetsCount
);
