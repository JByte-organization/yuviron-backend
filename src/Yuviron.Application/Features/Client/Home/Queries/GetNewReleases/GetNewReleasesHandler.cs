using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;

public sealed class GetNewReleasesHandler : IRequestHandler<GetNewReleasesQuery, List<NewReleaseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNewReleasesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NewReleaseDto>> Handle(GetNewReleasesQuery request, CancellationToken cancellationToken)
    {
        var dbAlbums = await _context.Albums
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(request.Limit)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.CoverUrl,
                ArtistNamesList = a.AlbumArtists.Select(aa => aa.Artist.Name).ToList(),
                TracksCount = a.Tracks.Count(t => !t.IsDeleted) 
            })
            .ToListAsync(cancellationToken);

        return dbAlbums
            .Select(a => new NewReleaseDto(
                a.Id,
                a.Title,
                string.Join(", ", a.ArtistNamesList),
                a.CoverUrl,
                a.TracksCount
            ))
            .ToList();
    }
}