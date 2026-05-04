using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

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

        var albumsData = await _context.Albums
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Title.Contains(searchTerm))
            .OrderBy(a => a.Title)
            .Take(request.Limit)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.CoverUrl,
                ArtistNamesList = a.AlbumArtists.Select(aa => aa.Artist.Name).ToList() 
            })
            .ToListAsync(cancellationToken);

        return albumsData
            .Select(a => new AlbumAutocompleteDto(
                a.Id,
                a.Title,
                string.Join(", ", a.ArtistNamesList),
                a.CoverUrl
            ))
            .ToList();
    }
}