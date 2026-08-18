using System.Text.Json.Serialization;

namespace Assura.Domain.Enums;

// [JsonConverter] use String instead of int when serializing to JSON, making it more readable and maintainable.
[JsonConverter(typeof(JsonStringEnumConverter))]

public enum RequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Passed = 4,
    TemporaryAssigned = 5,
    PendingProcurement = 6,
    PendingStorekeeperReview = 7,
    Cancelled = 8,
    Completed = 9,
    ApprovedProcument =10,
    DiscardedBySupirinton=11,
    AssetAssigned=12,
   
}