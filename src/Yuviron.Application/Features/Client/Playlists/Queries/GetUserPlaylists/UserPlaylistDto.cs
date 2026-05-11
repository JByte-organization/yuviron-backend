using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Playlist.Queries.GetUserPlaylists;

public sealed record UserPlaylistDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    int TracksCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsSystem = false
);
