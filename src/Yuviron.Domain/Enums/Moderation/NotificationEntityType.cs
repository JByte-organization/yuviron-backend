using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationEntityType 
{ 
    Unknown = 0,
    Track = 1, 
    Album = 2, 
    Artist = 3, 
    System = 99 
}