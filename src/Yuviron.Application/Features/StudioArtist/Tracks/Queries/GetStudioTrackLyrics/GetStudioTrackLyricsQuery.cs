using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTrackLyrics;

public sealed record GetStudioTrackLyricsQuery(Guid TrackId) : IRequest<StudioTrackLyricsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}