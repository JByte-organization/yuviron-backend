using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomRole
{
    Unknown = 0,
    Host = 1,
    Member = 2
}