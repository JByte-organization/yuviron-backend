using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Home.Queries.GetPersonalizedRecommendations;

public record RecommendationTrackDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    IEnumerable<TrackArtistDto> Artists
);
