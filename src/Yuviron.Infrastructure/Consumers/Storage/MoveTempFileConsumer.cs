using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class MoveTempFileConsumer : IConsumer<TempFileNeedsMovingEvent>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MoveTempFileConsumer> _logger;

    public MoveTempFileConsumer(IFileStorageService fileStorageService, ILogger<MoveTempFileConsumer> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TempFileNeedsMovingEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Background API Task: File Transfer {TempUrl} in {Folder}", message.TempUrl, message.DestinationFolder);

        await _fileStorageService.MoveIfTempAsync(message.TempUrl, message.DestinationFolder, context.CancellationToken);
    }
}