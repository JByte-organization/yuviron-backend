using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;

public sealed class GetPlaylistTracksHandler : IRequestHandler<GetPlaylistTracksQuery, PaginatedList<PlaylistTrackItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlaylistTracksHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<PlaylistTrackItemDto>> Handle(GetPlaylistTracksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .OrderBy(pt => pt.Position)
            .Select(pt => new PlaylistTrackItemDto(
                pt.TrackId,
                pt.Track.Title,
                pt.Track.TrackArtists.Select(ta => ta.Artist.Name).ToList(),
                pt.Track.AlbumId,
                pt.Track.Album != null ? pt.Track.Album.Title : "Unknown",
                pt.Track.CoverUrl ?? (pt.Track.Album != null ? pt.Track.Album.CoverUrl : null),
                pt.Track.DurationMs,
                pt.Position,
                pt.AddedAt
            ));

        return await query.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}