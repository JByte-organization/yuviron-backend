using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumsAutocomplete;

public sealed class GetAlbumsAutocompleteHandler : IRequestHandler<GetAlbumsAutocompleteQuery, List<AlbumAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlbumsAutocompleteHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlbumAutocompleteDto>> Handle(GetAlbumsAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<AlbumAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim();

        return await _context.Albums
            .AsNoTracking()
            .Where(a => a.Title.Contains(searchTerm))
            .OrderBy(a => a.Title)
            .Take(request.Limit)
            .Select(a => new AlbumAutocompleteDto(
                a.Id,
                a.Title,
                a.AlbumArtists
                    .Select(aa => new SimpleArtistDto(aa.ArtistId, aa.Artist.Name)),
                a.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}