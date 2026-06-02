using System;
using System.Collections.Generic;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;

public record StudioTrackListItemDto(
    Guid Id,
    Guid AlbumId,
    string AlbumTitle,
    int AlbumPosition,
    string Title,
    IEnumerable<string> ArtistNames,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    TrackProcessingStatus ProcessingStatus,
    VisibilityStatus VisibilityStatus,
    long TotalPlays,
    DateTime CreatedAt,
    DateTime UpdatedAt
);