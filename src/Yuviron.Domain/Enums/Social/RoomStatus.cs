using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomStatus
{
    Unknown = 0,
    Active = 1,
    Closed = 2
}