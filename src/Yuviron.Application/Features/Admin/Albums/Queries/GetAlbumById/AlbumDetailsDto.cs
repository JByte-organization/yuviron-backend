using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

public record AlbumDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    DateTime ReleaseDate,
    VisibilityStatus VisibilityStatus,
    DateTime? ScheduledPublishAt,
    int TracksCount,     
    long TotalDurationMs, 
    long TotalPlays,     
    DateTime CreatedAt,  
    DateTime UpdatedAt, 
    IEnumerable<SimpleArtistDto> Artists
);