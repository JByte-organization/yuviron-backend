using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountState
{
    Unknown = 0,
    Active = 1,
    Deleted = 2,
    Banned = 3
}