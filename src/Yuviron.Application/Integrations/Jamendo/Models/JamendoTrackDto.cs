using System.Text.Json.Serialization;

namespace Yuviron.Application.Integrations.Jamendo.Models;

public record JamendoTrackDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("duration")]
    public int DurationSeconds { get; init; }

    [JsonPropertyName("artist_id")]
    public string ArtistId { get; init; } = string.Empty;

    [JsonPropertyName("artist_name")]
    public string ArtistName { get; init; } = string.Empty;

    [JsonPropertyName("album_name")]
    public string AlbumName { get; init; } = string.Empty;

    // Ссылка на обложку (обычно 500x500)
    [JsonPropertyName("image")]
    public string CoverUrl { get; init; } = string.Empty;

    // Прямая ссылка на скачивание MP3
    [JsonPropertyName("audiodownload")]
    public string AudioDownloadUrl { get; init; } = string.Empty;
}