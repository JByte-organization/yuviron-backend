using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed record CreatePlaylistRequest(string Name, string? CoverUrl, PlaylistVisibility Visibility);
public sealed record CreatePlaylistResponse(Guid Id);

public sealed record CreatePlaylistCommand(
    string Name,
    string? CoverUrl,
    PlaylistVisibility Visibility
) : IRequest<Guid>;