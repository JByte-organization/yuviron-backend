using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistTracks;

public sealed record GetStudioPlaylistTracksQuery(
    Guid PlaylistId,
    int Page = 1,
    int PageSize = 50 
) : PaginatedQuery(null, null, null, Page, PageSize), 
    IRequest<PaginatedList<StudioPlaylistTrackItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}