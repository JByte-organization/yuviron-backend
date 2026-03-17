using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.ChangeTrackPosition;

public sealed class ChangeTrackPositionHandler : IRequestHandler<ChangeTrackPositionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public ChangeTrackPositionHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ChangeTrackPositionCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _context.Playlists
                           .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var playlistTrack = await _context.PlaylistTracks
                                .FirstOrDefaultAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken)
                            ?? throw new NotFoundException("PlaylistTrack", request.TrackId);

        playlistTrack.UpdatePosition(request.NewPosition);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        playlist.Update(playlist.Title, playlist.Description, playlist.CoverUrl, playlist.Visibility, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}