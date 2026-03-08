using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaybackSourceType
{
    Unknown = 0,
    Album = 1,
    Playlist = 2,
    ArtistProfile = 3,
    Search = 4,
    SavedTracks = 5,
    Recommendations = 6
}