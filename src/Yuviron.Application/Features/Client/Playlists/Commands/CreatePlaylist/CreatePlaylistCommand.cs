using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed record CreatePlaylistRequest(string Title, Guid? CoverFileId, PlaylistVisibility Visibility);
public sealed record CreatePlaylistResponse(Guid Id);

public sealed record CreatePlaylistCommand(
    string Title,
    Guid? CoverFileId,
    PlaylistVisibility Visibility
) : IRequest<Guid>;