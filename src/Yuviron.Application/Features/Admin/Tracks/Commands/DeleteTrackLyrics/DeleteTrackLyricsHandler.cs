using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrackLyrics;

public sealed class DeleteTrackLyricsHandler : IRequestHandler<DeleteTrackLyricsCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteTrackLyricsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTrackLyricsCommand request, CancellationToken cancellationToken)
    {
        var track = await _context.Tracks
                        .Include(t => t.Lyrics)
                        .FirstOrDefaultAsync(t => t.Id == request.TrackId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Track), request.TrackId);

        track.SetLyrics(null); 
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}