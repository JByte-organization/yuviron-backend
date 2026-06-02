using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;

public record StudioAlbumTrackDto(
    Guid Id,
    int Position,
    string Title,
    int DurationMs,
    bool Explicit,
    string? CoverUrl,
    TrackProcessingStatus ProcessingStatus,
    VisibilityStatus VisibilityStatus
);