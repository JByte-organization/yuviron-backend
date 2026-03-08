using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComplaintStatus 
{ 
    Unknown = 0,
    New = 1, 
    InReview = 2, 
    Approved = 3, 
    Rejected = 4 
}