using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.DTOs;

public record ArtistSimpleDto(Guid Id, string Name);
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
    int TotalPlays,     
    DateTime CreatedAt,  
    DateTime UpdatedAt, 
    List<ArtistSimpleDto> Artists
);