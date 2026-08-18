using System;

namespace Assura.Application.DTOs
{
    public class DivisionOverviewSummaryDto
    {
        public int AssetsCount { get; set; }
        public decimal AssetsPurchaseValue { get; set; }
        public int PendingRequestsCount { get; set; }
        public int TransferredAssetsCount { get; set; }
    }
}
