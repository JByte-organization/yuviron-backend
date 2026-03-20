using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Users.EventHandlers;

public sealed class ClearUserProfileEventHandler : INotificationHandler<UserDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public ClearUserProfileEventHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        // У тебя в UserProfile ключ Id совпадает с UserId (Id = userId в методе Create)
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == notification.UserId, cancellationToken);

        if (profile == null) return;

        // 1. Удаляем аватарку физически (файловый сервис сам проглотит ошибки, если они будут)
        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
        {
            await _fileStorageService.DeleteAsync(profile.AvatarUrl, cancellationToken);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        // 2. Анонимизируем данные в базе (вызываем метод, который мы только что добавили)
        profile.ClearPersonalData(utcNow);

        // 3. Сохраняем изменения
        await _context.SaveChangesAsync(cancellationToken);
    }
}