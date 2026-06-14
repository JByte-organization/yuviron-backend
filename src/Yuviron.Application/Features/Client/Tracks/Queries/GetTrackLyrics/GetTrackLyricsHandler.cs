using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Tracks.Queries.GetTrackLyrics;

public sealed class GetTrackLyricsHandler : IRequestHandler<GetTrackLyricsQuery, TrackLyricsDto>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider; 

    public GetTrackLyricsHandler(ICatalogContext catalogContext, TimeProvider timeProvider)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
    }

    public async Task<TrackLyricsDto> Handle(GetTrackLyricsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var trackData = await _catalogContext.Tracks
            .AsNoTracking()
            .AvailableForPublic(utcNow) 
            .Where(t => t.Id == request.TrackId)
            .Select(t => new TrackLyricsDto(t.Lyrics != null ? t.Lyrics.PlainText : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (trackData is null) throw new NotFoundException(nameof(Track), request.TrackId);

        return trackData;
    }
}