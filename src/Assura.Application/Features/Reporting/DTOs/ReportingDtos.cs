namespace Assura.Application.Features.Reporting.DTOs;

public class ReportingDashboardDto
{
    public List<ReportingMetricDto> Metrics { get; set; } = [];
    public List<ReportingLegendItemDto> CategoryLegend { get; set; } = [];
    public List<ReportingBarItemDto> StatusBars { get; set; } = [];
    public List<ReportingBarItemDto> DivisionBars { get; set; } = [];
    public List<ReportingBarItemDto> ValueBars { get; set; } = [];
    public ReportingAnomaliesDto Anomalies { get; set; } = new();
}

public class ReportingMetricDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Accent { get; set; }
}

public class ReportingLegendItemDto
{
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportingBarItemDto
{
    public string Label { get; set; } = string.Empty;
    public decimal RawValue { get; set; }
    public decimal Value { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class ReportingAnomaliesDto
{
    public int GhostAssetsDetected { get; set; }
    public int MissingPhysicalVerification { get; set; }
}

public class ReportingAuditLogPageDto
{
    public List<ReportingStatCardDto> Stats { get; set; } = [];
    public List<ReportingAuditLogEntryDto> Logs { get; set; } = [];
}

public class ReportingStatCardDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
}

public class ReportingAuditLogEntryDto
{
    public string Time { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ReportingAssetsPageDto
{
    public int SelectedCount { get; set; }
    public List<ReportingAssetRowDto> Assets { get; set; } = [];
}

public class ReportingAssetRowDto
{
    public int Id { get; set; }
    public string AssetId { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public string Swatch { get; set; } = string.Empty;
    public string ImageClass { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CheckedBy { get; set; }
    public string? CheckedRole { get; set; }
    public string AssuraName { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string Warranty { get; set; } = string.Empty;
    public string EndOfLife { get; set; } = string.Empty;
    public string CodeNumber { get; set; } = string.Empty;
}

public class ReportingReportsPageDto
{
    public List<ReportingStatCardDto> Summaries { get; set; } = [];
    public List<ReportingReportItemDto> ReportItems { get; set; } = [];
    public List<ReportingInsightDto> Insights { get; set; } = [];
}

public class ReportingReportItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Generated { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}

public class ReportingInsightDto
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
}
