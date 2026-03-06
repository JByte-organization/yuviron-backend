using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Tracks.EventHandlers;

public sealed class RemoveTrackFromPlaylistsEventHandler : INotificationHandler<TrackDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public RemoveTrackFromPlaylistsEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(TrackDeletedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Находим все записи в плейлистах пользователей, где есть этот трек
        // Предполагается, что в IApplicationDbContext есть DbSet<PlaylistTrack> PlaylistTracks;
        var playlistLinks = await _context.PlaylistTracks
            .Where(pt => pt.TrackId == notification.TrackId)
            .ToListAsync(cancellationToken);

        if (!playlistLinks.Any())
        {
            return;
        }

        // 2. Для связующих таблиц Many-to-Many мы обычно используем жесткое удаление (Hard Delete),
        // так как хранить "мягко удаленную" связь трека и плейлиста не имеет смысла, это просто мусор.
        _context.PlaylistTracks.RemoveRange(playlistLinks);

    }
}