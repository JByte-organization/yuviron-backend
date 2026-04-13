using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistById;

public sealed record PlaylistDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    bool IsEditorial,
    string CreatorName,
    Guid? CreatorId, 
    int TracksCount,  
    DateTime CreatedAt, 
    DateTime UpdatedAt
);