using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylists;

public record StudioPlaylistListItemDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    DateTime CreatedAt
);