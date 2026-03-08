using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArtistTeamRole
{
    Unknown = 0,
    Owner = 1,
    Manager = 2,
    Editor = 3,
    Viewer = 4
}