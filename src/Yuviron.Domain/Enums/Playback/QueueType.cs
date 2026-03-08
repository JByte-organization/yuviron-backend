using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueueType
{
    Unknown = 0,
    Next = 1,
    Normal = 2
}