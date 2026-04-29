using System.Text.Json.Serialization;

namespace Yuviron.Application.Integrations.Jamendo.Models;

public class JamendoTrackDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("artist_id")]
    public required string ArtistId { get; set; }

    [JsonPropertyName("artist_name")]
    public required string ArtistName { get; set; }

    [JsonPropertyName("album_name")]
    public required string AlbumName { get; set; }

    [JsonPropertyName("album_id")]
    public required string AlbumId { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("audiodownload")]
    public required string AudioDownloadUrl { get; set; }

    [JsonPropertyName("image")]
    public required string CoverUrl { get; set; }
    
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; } 

    [JsonPropertyName("musicinfo")]
    public JamendoMusicInfo? MusicInfo { get; set; }
}

public class JamendoMusicInfo
{
    [JsonPropertyName("tags")]
    public JamendoTags? Tags { get; set; }
}

public class JamendoTags
{
    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }
    
    [JsonPropertyName("instruments")]
    public List<string>? Instruments { get; set; }
    
    [JsonPropertyName("vartags")]
    public List<string>? Moods { get; set; } 
}