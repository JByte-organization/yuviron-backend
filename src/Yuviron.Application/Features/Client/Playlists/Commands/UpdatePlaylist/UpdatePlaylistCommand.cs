using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed record UpdatePlaylistRequest(
    string? Title, 
    Guid? CoverFileId,
    PlaylistVisibility? Visibility 
);

public sealed record UpdatePlaylistCommand(
    Guid PlaylistId,
    string? Title,
    Guid? CoverFileId,
    PlaylistVisibility? Visibility
) : IRequest;