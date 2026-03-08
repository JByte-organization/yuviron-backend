using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    NotSpecified = 0,
    Male = 1,
    Female = 2,
    NonBinary = 3,
    Other = 4
}