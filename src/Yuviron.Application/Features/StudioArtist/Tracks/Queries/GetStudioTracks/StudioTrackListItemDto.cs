using System;
using System.Collections.Generic;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;

public record StudioTrackListItemDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    int DurationMs,
    TrackProcessingStatus ProcessingStatus,
    VisibilityStatus VisibilityStatus,
    long PlayCount,
    DateTime CreatedAt
);