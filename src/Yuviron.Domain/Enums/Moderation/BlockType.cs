using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BlockType
{
    Unknown = 0,
    Login = 1,
    Playback = 2,
    Upload = 3,
    Monetization = 4,
    Full = 99
}