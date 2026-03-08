using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionStatus
{
    Unknown = 0,

    Pending = 1,

    Active = 2,

    Cancelled = 3,

    Expired = 4,

    PastDue = 5
}