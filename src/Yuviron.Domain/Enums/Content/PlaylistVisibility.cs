using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaylistVisibility
{
    Private = 0,  // Only the creator sees
    Public = 1,   // Seen by everyone, displayed in search
    Unlisted = 2  // Access only via direct link (hidden from search)
}