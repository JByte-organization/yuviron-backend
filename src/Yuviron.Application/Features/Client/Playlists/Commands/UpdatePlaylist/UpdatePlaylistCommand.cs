using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed record UpdatePlaylistRequest(
    string? Name, 
    string? CoverUrl, 
    PlaylistVisibility? Visibility 
);

public sealed record UpdatePlaylistCommand(
    Guid PlaylistId,
    string? Name,
    string? CoverUrl,
    PlaylistVisibility? Visibility
) : IRequest;