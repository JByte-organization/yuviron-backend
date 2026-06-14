using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

public sealed class GetTracksAutocompleteHandler : IRequestHandler<GetTracksAutocompleteQuery, List<TrackAutocompleteDto>>
{
    private readonly ICatalogContext _catalogContext;

    public GetTracksAutocompleteHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<List<TrackAutocompleteDto>> Handle(GetTracksAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<TrackAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim(); 

        return await _catalogContext.Tracks
            .AsNoTracking()
            .Where(t => t.Title.Contains(searchTerm)) 
            .OrderBy(t => t.Title)
            .Take(request.Limit)
            .Select(t => new TrackAutocompleteDto(
                t.Id,
                t.Title,
                t.TrackArtists
                    .Select(ta => new SimpleArtistDto(ta.ArtistId, ta.Artist.Name)), 
                t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null)
            ))
            .ToListAsync(cancellationToken);
    }
}