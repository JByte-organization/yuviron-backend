using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerificationRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}