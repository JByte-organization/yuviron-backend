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
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetMoodsHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<List<MoodItemDto>> Handle(GetMoodsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var availableTracks = _context.Tracks.AvailableForPublic(utcNow);

        return await _context.Moods
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
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