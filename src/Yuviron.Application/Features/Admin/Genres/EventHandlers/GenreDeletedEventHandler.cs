using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Genres.EventHandlers;

public sealed class GenreDeletedEventHandler : INotificationHandler<GenreDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public GenreDeletedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(GenreDeletedEvent notification, CancellationToken cancellationToken)
    {
        var trackGenresToRemove = await _context.TrackGenres
            .Where(tg => tg.GenreId == notification.GenreId)
            .ToListAsync(cancellationToken);

        if (!trackGenresToRemove.Any())
        {
            return;
        }

        _context.TrackGenres.RemoveRange(trackGenresToRemove);
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}