using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class ClearUserProfileConsumer : IConsumer<UserDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public ClearUserProfileConsumer(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var profile = await _context.UserProfiles
            .FirstAsync(p => p.Id == context.Message.UserId, context.CancellationToken);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        profile.ClearPersonalData(utcNow);

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}