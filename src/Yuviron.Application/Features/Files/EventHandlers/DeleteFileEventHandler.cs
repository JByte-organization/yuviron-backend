using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Files.EventHandlers;

public sealed class DeleteFileEventHandler : INotificationHandler<FileNeedsDeletionEvent>
{
    private readonly IFileStorageService _fileStorageService;

    public DeleteFileEventHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task Handle(FileNeedsDeletionEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.FileUrl))
        {
            return; 
        }

        await _fileStorageService.DeleteAsync(notification.FileUrl, cancellationToken);
    }
}