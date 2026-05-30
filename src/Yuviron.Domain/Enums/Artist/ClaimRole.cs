using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClaimRole
{
    Artist = 1,
    Manager = 2,
    Label = 3
}