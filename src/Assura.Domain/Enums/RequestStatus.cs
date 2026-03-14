using System.Text.Json.Serialization;

namespace Assura.Domain.Enums;

// [JsonConverter] එක භාවිතා කිරීමෙන් ඉලක්කම් වෙනුවට නම (String) යවනු ලැබේ
[JsonConverter(typeof(JsonStringEnumConverter))]

public enum RequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}