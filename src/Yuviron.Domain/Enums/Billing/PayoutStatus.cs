using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayoutStatus 
{ 
    Unknown = 0,
    Pending = 1, 
    Approved = 2, 
    Rejected = 3, 
    Paid = 4 
}