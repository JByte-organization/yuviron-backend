using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionStatus 
{ 
    Unknown = 0,
    Success = 1, 
    Failed = 2 
}