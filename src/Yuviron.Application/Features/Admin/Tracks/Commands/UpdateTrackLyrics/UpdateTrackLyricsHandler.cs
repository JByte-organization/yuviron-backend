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
    private readonly IApplicationDbContext _context;

    public UpdateTrackLyricsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTrackLyricsCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
                        .Include(t => t.Lyrics)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        track.SetLyrics(request.LyricsText);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}