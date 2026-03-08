using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SmartLinkType
{
    Unknown = 0,
    Track = 1,
    Album = 2,
    Artist = 3
}