using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyUserOnPasswordChangedConsumer : IConsumer<UserPasswordChangedEvent>
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public NotifyUserOnPasswordChangedConsumer(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<UserPasswordChangedEvent> context)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == context.Message.UserId, context.CancellationToken);

        if (user == null) return;

        await _emailService.SendEmailAsync(
            user.Email,
            "Пароль змінено - Yuviron",
            "<p>Пароль вашого облікового запису було успішно змінено.</p><p>Якщо це були не ви, негайно змініть пароль та зв'яжіться з підтримкою.</p>",
            context.CancellationToken);
    }
}
