using System.Threading.Channels;
using Yuviron.Application.Abstractions.Messaging;

namespace Yuviron.Infrastructure.Services;

public class EmailJobQueue : IEmailJobQueue
{
    private readonly Channel<EmailJob> _queue;

    public EmailJobQueue()
    {
        // Ограничиваем очередь, например, в 1000 писем, чтобы не переполнить память
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<EmailJob>(options);
    }

    public async ValueTask EnqueueEmailAsync(EmailJob job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<EmailJob> DequeueEmailAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}