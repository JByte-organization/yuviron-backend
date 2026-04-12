using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Tracks.EventHandlers;

public sealed class DirectoryNeedsDeletionEventHandler : INotificationHandler<DirectoryNeedsDeletionEvent>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DirectoryNeedsDeletionEventHandler> _logger;

    public DirectoryNeedsDeletionEventHandler(
        IFileStorageService fileStorageService, 
        ILogger<DirectoryNeedsDeletionEventHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Handle(DirectoryNeedsDeletionEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _fileStorageService.DeleteDirectoryAsync(notification.DirectoryPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при фоновом удалении директории {DirectoryPath}", notification.DirectoryPath);
            throw;
        }
    }
}