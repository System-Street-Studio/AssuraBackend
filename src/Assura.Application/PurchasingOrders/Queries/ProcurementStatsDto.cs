namespace Assura.Application.PurchasingOrders.Queries;

public class ProcurementStatsDto
{
    public int TotalSuppliers { get; set; }
    public int PosNotCompleted { get; set; }
    public int PosCompleted { get; set; }
    public int RepairsNotCompleted { get; set; }
    public int RepairsCompleted { get; set; }
}
