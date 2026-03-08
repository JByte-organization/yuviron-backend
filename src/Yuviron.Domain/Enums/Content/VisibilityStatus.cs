using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VisibilityStatus
{
    Draft = 0,
    Scheduled = 1,
    Published = 2,
    Hidden = 3
}