using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArtistRole
{
    Unknown = 0,
    Main = 1,
    Feat = 2,
    Producer = 3
}