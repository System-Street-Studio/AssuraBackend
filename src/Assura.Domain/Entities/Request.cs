using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class Request : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public RequestType Type { get; set; }
    public PriorityType Priority { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Remarks { get; set; }
    public string? Specifications { get; set; }
    public string? SpecialNote { get; set; }

    public int RequesterId { get; set; }
    public User Requester { get; set; } = null!;

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }
}
