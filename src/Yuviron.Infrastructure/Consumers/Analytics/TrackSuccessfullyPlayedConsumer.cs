using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class TrackSuccessfullyPlayedConsumer : IConsumer<TrackSuccessfullyPlayedEvent>
{
    public TrackSuccessfullyPlayedConsumer()
    {
    }

    public async Task Consume(ConsumeContext<TrackSuccessfullyPlayedEvent> context)
    {
        var msg = context.Message;

        //оставляем для будущего действия
        await Task.CompletedTask;
    }
}