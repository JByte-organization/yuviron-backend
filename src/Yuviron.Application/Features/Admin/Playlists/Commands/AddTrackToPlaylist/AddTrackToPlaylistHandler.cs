using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistHandler : IRequestHandler<AddTrackToPlaylistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public AddTrackToPlaylistHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AddTrackToPlaylistCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("User context is required.");

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == request.PlaylistId, cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        var trackExists = await _context.Tracks.AnyAsync(t => t.Id == request.TrackId, cancellationToken);
        if (!trackExists) throw new NotFoundException(nameof(Track), request.TrackId);

        if (await _context.PlaylistTracks.AnyAsync(pt => pt.PlaylistId == request.PlaylistId && pt.TrackId == request.TrackId, cancellationToken))
            return Unit.Value;

        int maxPosition = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (int?)pt.Position, cancellationToken) ?? 0;

        int insertPosition = (request.Position <= 0 || request.Position > maxPosition + 1) 
            ? maxPosition + 1 
            : request.Position;

        // ЧИСТАЯ ТРАНЗАКЦИЯ БЕЗ КОСТЫЛЕЙ
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            if (insertPosition <= maxPosition)
            {
                await _context.PlaylistTracks
                    .Where(pt => pt.PlaylistId == request.PlaylistId && pt.Position >= insertPosition)
                    .ExecuteUpdateAsync(s => s.SetProperty(pt => pt.Position, pt => pt.Position + 1), cancellationToken);
            }

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var newPlaylistTrack = new PlaylistTrack(request.PlaylistId, request.TrackId, insertPosition, currentUserId, utcNow);
            
            _context.PlaylistTracks.Add(newPlaylistTrack);
            playlist.NotifyContentChanged(utcNow);

            await _context.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Unit.Value;
    }
}