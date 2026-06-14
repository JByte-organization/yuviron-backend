using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTrackLyrics;

public sealed class GetTrackLyricsHandler : IRequestHandler<GetTrackLyricsQuery, AdminTrackLyricsDto>
{
    private readonly ICatalogContext _catalogContext;

    public GetTrackLyricsHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<AdminTrackLyricsDto> Handle(GetTrackLyricsQuery request, CancellationToken cancellationToken)
    {
        var trackData = await _catalogContext.Tracks
            .AsNoTracking()
            .Where(t => t.Id == request.TrackId)
            .Select(t => new AdminTrackLyricsDto(t.Lyrics != null ? t.Lyrics.PlainText : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (trackData is null) throw new NotFoundException(nameof(Track), request.TrackId);

        return trackData;
    }
}