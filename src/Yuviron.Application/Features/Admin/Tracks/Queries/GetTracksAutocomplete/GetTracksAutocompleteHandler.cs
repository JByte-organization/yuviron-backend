using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

public sealed class GetTracksAutocompleteHandler : IRequestHandler<GetTracksAutocompleteQuery, List<TrackAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTracksAutocompleteHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrackAutocompleteDto>> Handle(GetTracksAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<TrackAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim().ToLower();

        var tracksData = await _context.Tracks
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.Title.ToLower().Contains(searchTerm))
            .OrderBy(t => t.Title)
            .Take(request.Limit)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.CoverUrl,
                AlbumCoverUrl = t.Album.CoverUrl,
                ArtistNamesList = t.TrackArtists.Select(ta => ta.Artist.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return tracksData
            .Select(t => new TrackAutocompleteDto(
                t.Id,
                t.Title,
                string.Join(", ", t.ArtistNamesList), 
                t.CoverUrl ?? t.AlbumCoverUrl 
            ))
            .ToList();
    }
}