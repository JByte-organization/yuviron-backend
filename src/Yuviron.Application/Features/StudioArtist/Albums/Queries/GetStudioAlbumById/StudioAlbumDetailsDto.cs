using System;
using System.Collections.Generic;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;

public record StudioAlbumDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    DateTime ReleaseDate,
    ReleaseType ReleaseType,
    VisibilityStatus VisibilityStatus,
    DateTime? ScheduledPublishAt,
    int TracksCount,     
    long TotalDurationMs, 
    long TotalPlays,     
    DateTime CreatedAt,  
    DateTime UpdatedAt, 
    IEnumerable<TrackArtistDto> Artists
);