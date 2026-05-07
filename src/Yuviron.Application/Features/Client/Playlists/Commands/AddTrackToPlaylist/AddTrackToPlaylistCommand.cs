using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;

public sealed record AddTrackRequest(Guid TrackId);
public sealed record AddTrackToPlaylistCommand(Guid PlaylistId, Guid TrackId) : IRequest;