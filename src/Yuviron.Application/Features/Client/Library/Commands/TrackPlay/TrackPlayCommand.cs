using MediatR;

namespace Yuviron.Application.Features.Client.Library.Commands.TrackPlay;

public sealed record TrackPlayRequest(Guid TrackId);

public sealed record TrackPlayCommand(Guid TrackId, string? CountryCode) : IRequest<Unit>;
