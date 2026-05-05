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
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetGenresHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<GenreItemDto>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var availableTracks = _context.Tracks.AvailableForPublic(utcNow);

        return await _context.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted)
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