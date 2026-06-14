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

namespace Yuviron.Application.Features.Client.Moods.Queries.GetMoods;

public sealed class GetMoodsHandler : IRequestHandler<GetMoodsQuery, List<MoodItemDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;

    public GetMoodsHandler(ICatalogContext catalogContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<List<MoodItemDto>> Handle(GetMoodsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var availableTracks = _catalogContext.Tracks.AvailableForPublic(utcNow);

        return await _catalogContext.Moods
            .AsNoTracking()
            
            .Where(m => availableTracks.Any(t => t.TrackMoods.Any(tm => tm.MoodId == m.Id)))
            .OrderBy(m => m.Name) 
            .Take(request.Limit)
            .Select(m => new MoodItemDto(
                m.Id,
                m.Name,
                m.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}