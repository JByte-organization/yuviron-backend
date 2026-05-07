using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Commands.DeletePlaylist;

public sealed record DeletePlaylistCommand(Guid PlaylistId) : IRequest;