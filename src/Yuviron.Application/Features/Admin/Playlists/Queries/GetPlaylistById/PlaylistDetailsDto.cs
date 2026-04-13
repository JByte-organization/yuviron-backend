using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistById;

public sealed record PlaylistCreatorDto(
    Guid? Id, 
    string? Name, 
    string? Email,
    string? AvatarUrl
);

public sealed record PlaylistDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    bool IsEditorial,
    PlaylistCreatorDto? Creator, 
    int TracksCount,  
    DateTime CreatedAt, 
    DateTime UpdatedAt
);