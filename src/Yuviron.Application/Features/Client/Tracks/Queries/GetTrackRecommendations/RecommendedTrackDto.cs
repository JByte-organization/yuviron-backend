using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models; 

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackRecommendations;

public record RecommendedTrackDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string? AudioUrl,
    IEnumerable<TrackArtistDto> Artists 
);