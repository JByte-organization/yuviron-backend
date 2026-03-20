using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Files.EventHandlers;

public sealed class MoveTempFileEventHandler : INotificationHandler<TempFileNeedsMovingEvent>
{
    private readonly IFileStorageService _fileStorageService;

    public MoveTempFileEventHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task Handle(TempFileNeedsMovingEvent notification, CancellationToken cancellationToken)
    {
        // Вызываем перемещение. Если файла нет, вылетит ошибка, 
        // Outbox ее поймает и сделает Retry
        await _fileStorageService.MoveIfTempAsync(notification.TempUrl, notification.DestinationFolder, cancellationToken);
    }
}