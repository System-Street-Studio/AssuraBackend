using System.Text.Json.Serialization;

namespace Assura.Domain.Enums;

// [JsonConverter] use String instead of int when serializing to JSON, making it more readable and maintainable.
[JsonConverter(typeof(JsonStringEnumConverter))]

public enum RequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}