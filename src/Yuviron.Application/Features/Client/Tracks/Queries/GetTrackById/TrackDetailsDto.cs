using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;

public record TrackDetailsDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    string? AudioUrl,
    long PlayCount,
    
    Guid AlbumId,
    string AlbumTitle,
    int AlbumPosition,
    
    List<TrackArtistDto> Artists,
    List<string> Genres,
    List<string> Moods
);

public record TrackArtistDto(
    Guid Id,
    string Name,
    string Role
);