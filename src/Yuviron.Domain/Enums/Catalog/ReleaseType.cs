using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReleaseType
{
    Single = 1,
    EP = 2,
    Album = 3
}
