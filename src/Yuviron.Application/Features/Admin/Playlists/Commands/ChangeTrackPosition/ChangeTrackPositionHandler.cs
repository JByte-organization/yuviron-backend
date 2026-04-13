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

        var trackToMove = await _context.PlaylistTracks
            .FirstOrDefaultAsync(t => t.PlaylistId == request.PlaylistId && t.TrackId == request.TrackId, cancellationToken)
            ?? throw new NotFoundException("PlaylistTrack", request.TrackId);

        int maxPosition = await _context.PlaylistTracks
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .MaxAsync(pt => (int?)pt.Position, cancellationToken) ?? 0;

        int oldPosition = trackToMove.Position;
        int newPosition = Math.Clamp(request.NewPosition, 1, maxPosition);

        if (oldPosition == newPosition) return Unit.Value;

        // ИСПОЛЬЗУЕМ НАШУ АБСТРАКЦИЮ
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Временная позиция
            trackToMove.UpdatePosition(-1);
            await _context.SaveChangesAsync(cancellationToken);

            // 2. Сдвиг остальных
            if (newPosition < oldPosition)
            {
                await _context.PlaylistTracks
                    .Where(pt => pt.PlaylistId == request.PlaylistId && pt.Position >= newPosition && pt.Position < oldPosition)
                    .ExecuteUpdateAsync(s => s.SetProperty(pt => pt.Position, pt => pt.Position + 1), cancellationToken);
            }
            else
            {
                await _context.PlaylistTracks
                    .Where(pt => pt.PlaylistId == request.PlaylistId && pt.Position > oldPosition && pt.Position <= newPosition)
                    .ExecuteUpdateAsync(s => s.SetProperty(pt => pt.Position, pt => pt.Position - 1), cancellationToken);
            }

            // 3. Финальная установка
            trackToMove.UpdatePosition(newPosition);
            playlist.NotifyContentChanged(_timeProvider.GetUtcNow().UtcDateTime);

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