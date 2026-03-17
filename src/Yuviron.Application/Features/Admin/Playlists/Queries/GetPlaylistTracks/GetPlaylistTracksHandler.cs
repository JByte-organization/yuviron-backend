using MediatR;
using Microsoft.EntityFrameworkCore;
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
                pt.Track.TrackArtists.FirstOrDefault() != null 
                    ? pt.Track.TrackArtists.FirstOrDefault()!.Artist.Name 
                    : null,
                pt.Position,
                pt.AddedAt
            ));

        return await query.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}