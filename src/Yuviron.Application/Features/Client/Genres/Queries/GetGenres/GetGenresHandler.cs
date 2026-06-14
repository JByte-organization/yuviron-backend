using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Genres.Queries.GetGenres;

public sealed class GetGenresHandler : IRequestHandler<GetGenresQuery, List<GenreItemDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;

    public GetGenresHandler(ICatalogContext catalogContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<List<GenreItemDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var availableTracks = _catalogContext.Tracks.AvailableForPublic(utcNow);

        return await _catalogContext.Genres
            .AsNoTracking()
            
            .Where(g => availableTracks.Any(t => t.TrackGenres.Any(tg => tg.GenreId == g.Id)))
            .OrderBy(g => g.Name) 
            .Take(request.Limit)
            .Select(g => new GenreItemDto(
                g.Id,
                g.Name,
                g.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}