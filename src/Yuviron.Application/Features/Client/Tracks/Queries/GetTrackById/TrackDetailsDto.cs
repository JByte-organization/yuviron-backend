using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackById;

public record TrackDetailsDto(
    Guid Id,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    long PlayCount,
    
    Guid AlbumId,
    string AlbumTitle,
    int AlbumPosition,
    
    IEnumerable<TrackArtistDto> Artists,
    IEnumerable<string> Genres,
    IEnumerable<string> Moods
);