using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed record RemoveTrackFromPlaylistCommand(Guid PlaylistId, Guid TrackId) : IRequest;