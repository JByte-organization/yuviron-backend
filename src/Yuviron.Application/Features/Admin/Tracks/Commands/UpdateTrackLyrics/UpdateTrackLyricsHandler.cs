using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrackLyrics;

public sealed class UpdateTrackLyricsHandler : IRequestHandler<UpdateTrackLyricsCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;

    public UpdateTrackLyricsHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<Unit> Handle(UpdateTrackLyricsCommand request, CancellationToken cancellationToken)
    {
        var track = await _catalogContext.Tracks
                        .Include(t => t.Lyrics)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        track.SetLyrics(request.LyricsText);

        await _catalogContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}