using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlanType
{
    Unknown = 0,
    Listener = 1, 
    Artist = 2   
}