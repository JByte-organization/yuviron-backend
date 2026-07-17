using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SmartLinkType
{
    Artist = 1,
    Album = 2,
    Track = 3,
    Playlist = 4,
    UserProfile = 5
}
