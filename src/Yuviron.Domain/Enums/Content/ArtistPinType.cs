using System.Text.Json.Serialization;

namespace Yuviron.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum ArtistPinType 
{ 
    Unknown = 0, 
    Track = 1, 
    Album = 2 ,
    Playlist = 3
}