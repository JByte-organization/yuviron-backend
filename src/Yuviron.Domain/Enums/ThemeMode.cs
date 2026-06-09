using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ThemeMode
{
    System = 1,
    Dark = 2,
    White = 3
}
