using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Moods.EventHandlers;

public sealed class MoodDeletedEventHandler : INotificationHandler<MoodDeletedEvent>
{
    private readonly IApplicationDbContext _context;

    public MoodDeletedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MoodDeletedEvent notification, CancellationToken cancellationToken)
    {
        var trackMoodsToRemove = await _context.TrackMoods
            .Where(tm => tm.MoodId == notification.MoodId)
            .ToListAsync(cancellationToken);

        if (!trackMoodsToRemove.Any()) return;

        _context.TrackMoods.RemoveRange(trackMoodsToRemove);
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}