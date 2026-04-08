using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Yuviron.Application.Integrations.Jamendo.Models;

public record JamendoResponse
{
    [JsonPropertyName("headers")]
    public JamendoHeaders Headers { get; init; } = new();

    [JsonPropertyName("results")]
    public List<JamendoTrackDto> Results { get; init; } = new();
}

public record JamendoHeaders
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("results_count")]
    public int ResultsCount { get; init; }
}