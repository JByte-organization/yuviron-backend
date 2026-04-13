using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class DeleteDirectoryConsumer : IConsumer<DirectoryNeedsDeletionEvent>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteDirectoryConsumer> _logger;

    public DeleteDirectoryConsumer(IFileStorageService fileStorageService, ILogger<DeleteDirectoryConsumer> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DirectoryNeedsDeletionEvent> context)
    {
        if (string.IsNullOrWhiteSpace(context.Message.DirectoryPath)) return;

        _logger.LogInformation("Background API Task: Delete a Directory {DirectoryPath}", context.Message.DirectoryPath);
        await _fileStorageService.DeleteDirectoryAsync(context.Message.DirectoryPath, context.CancellationToken);
    }
}