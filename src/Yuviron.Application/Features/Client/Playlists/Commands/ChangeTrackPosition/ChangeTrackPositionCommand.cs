using MediatR;

namespace Yuviron.Application.Features.Client.Playlists.Commands.ChangeTrackPosition;

public sealed record ChangeTrackPositionRequest(double NewPosition);
public sealed record ChangeTrackPositionCommand(Guid PlaylistId, Guid TrackId, double NewPosition) : IRequest;