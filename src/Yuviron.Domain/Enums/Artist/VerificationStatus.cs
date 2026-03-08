using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerificationStatus
{
    None = 0,
    Pending = 1,
    Verified = 2,
    Rejected = 3
}