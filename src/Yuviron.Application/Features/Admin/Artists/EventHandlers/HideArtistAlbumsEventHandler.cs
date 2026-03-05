using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Artists.EventHandlers;

public sealed class HideArtistAlbumsEventHandler : INotificationHandler<ArtistDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public HideArtistAlbumsEventHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(ArtistDeletedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Ищем все альбомы, в которых этот артист принимал участие
        var albumsToHide = await _context.Albums
            .Where(a => a.AlbumArtists.Any(aa => aa.ArtistId == notification.ArtistId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!albumsToHide.Any())
        {
            return; // Если альбомов нет, ничего не делаем
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 2. Делаем Soft Delete каждому альбому.
        // ВНИМАНИЕ: Внутри метода Delete() альбом сгенерирует AlbumDeletedEvent,
        // который автоматически удалит все треки этого альбома! Магия! 🪄
        foreach (var album in albumsToHide)
        {
            album.Delete(utcNow);
        }

        // 3. Сохраняем изменения (это сохранит альбомы и отправит новые события)
        await _context.SaveChangesAsync(cancellationToken);
    }
}