using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Queries;

public sealed record PlaylistDto(
    Guid Id,
    string Title,
    string? CoverUrl, 
    PlaylistVisibility Visibility,
    bool IsEditorial, 
    string CreatorName,
    int TracksCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);