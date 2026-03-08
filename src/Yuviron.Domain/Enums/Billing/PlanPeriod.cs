using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlanPeriod 
{ 
    Unknown = 0,
    Month = 1, 
    Year = 2 
}