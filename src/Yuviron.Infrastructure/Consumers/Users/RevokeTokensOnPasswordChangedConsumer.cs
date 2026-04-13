using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class RevokeTokensOnPasswordChangedConsumer : IConsumer<UserPasswordChangedEvent>
{
    private readonly IApplicationDbContext _context;

    public RevokeTokensOnPasswordChangedConsumer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<UserPasswordChangedEvent> context)
    {
        var tokensToRemove = await _context.RefreshTokens
            .Where(rt => rt.UserId == context.Message.UserId)
            .ToListAsync(context.CancellationToken);

        if (!tokensToRemove.Any()) return; 

        _context.RefreshTokens.RemoveRange(tokensToRemove);
        
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}