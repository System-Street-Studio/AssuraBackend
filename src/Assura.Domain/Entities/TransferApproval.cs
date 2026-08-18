using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class TransferApproval : BaseEntity
{
    public int TransferId { get; set; }
    public Transfer Transfer { get; set; } = null!;

    public int ApprovedByUserId { get; set; }
    public User ApprovedByUser { get; set; } = null!;

    public TransferStatus FromStatus { get; set; }
    public TransferStatus ToStatus { get; set; }
    
    public string? Comments { get; set; }
    
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
}
