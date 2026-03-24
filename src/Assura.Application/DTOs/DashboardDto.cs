namespace Assura.Application.DTOs;

public class DashboardDto
{
    public KpiDto Kpis { get; set; } = new();
    public ChartDatasetsDto Charts { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
    public List<WarrantyAlertDto> WarrantyAlerts { get; set; } = new();
}

public class KpiDto
{
    public int TotalAssets { get; set; }
    public int CheckedOut { get; set; }
    public int Available { get; set; }
    public string TotalAssetValue { get; set; } = "$0";
    public int PendingRequests { get; set; }
    public int MaintenanceDue { get; set; }
}

public class RecentActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // checked_out, checked_in, registered, maintenance, disposed, transferred
    public string AssetName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = "info";
    public string Color { get; set; } = "#0b6c78";
}

public class WarrantyAlertDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Severity { get; set; } = "info"; // critical, warning, info
}

public class ChartDatasetsDto
{
    public ChartDataDto AssetsByCategory { get; set; } = new();
    public ChartDataDto AssetsByStatus { get; set; } = new();
    public ChartDataDto AssetsByDepartment { get; set; } = new();
    public ChartDataDto CheckoutTrend { get; set; } = new();
    public AnomaliesDto Anomalies { get; set; } = new();
}

public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<int> Data { get; set; } = new();
    public List<string> Colors { get; set; } = new();
}

public class AnomaliesDto
{
    public int GhostAssets { get; set; }
    public int MissingAssets { get; set; }
    public int MaintenanceDue { get; set; }
}
