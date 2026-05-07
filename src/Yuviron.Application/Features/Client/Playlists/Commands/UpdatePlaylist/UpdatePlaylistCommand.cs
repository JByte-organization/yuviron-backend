using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed record UpdatePlaylistRequest(string? Name, string? CoverUrl);
public sealed record UpdatePlaylistCommand(
    Guid PlaylistId,
    string? Name,
    string? CoverUrl
) : IRequest;