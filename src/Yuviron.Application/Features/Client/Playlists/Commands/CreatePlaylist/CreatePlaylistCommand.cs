using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed record CreatePlaylistRequest(string Name, string? CoverUrl);
public sealed record CreatePlaylistResponse(Guid Id);

public sealed record CreatePlaylistCommand(
    string Name,
    string? CoverUrl
) : IRequest<Guid>;