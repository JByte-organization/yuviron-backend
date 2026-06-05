using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrackLyrics;

public sealed record UpdateTrackLyricsCommand(Guid TrackId, string LyricsText) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}