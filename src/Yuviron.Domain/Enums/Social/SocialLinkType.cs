using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SocialLinkType
{
    Website = 0,
    YouTube = 1,
    Instagram = 2,
    Twitter = 3,
    Facebook = 4,
    TikTok = 5,
    Spotify = 6,
    AppleMusic = 7,
    SoundCloud = 8
}