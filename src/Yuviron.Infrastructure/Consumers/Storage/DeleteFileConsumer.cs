using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class DeleteFileConsumer : IConsumer<FileNeedsDeletionEvent>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteFileConsumer> _logger;

    public DeleteFileConsumer(IFileStorageService fileStorageService, ILogger<DeleteFileConsumer> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FileNeedsDeletionEvent> context)
    {
        if (string.IsNullOrWhiteSpace(context.Message.FileUrl)) return;

        _logger.LogInformation("Background Task API: Delete File {FileUrl}", context.Message.FileUrl);
        await _fileStorageService.DeleteAsync(context.Message.FileUrl, context.CancellationToken);
    }
}