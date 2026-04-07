namespace Yuviron.Application.Abstractions.Messaging;

public interface IEmailJobQueue
{
    ValueTask EnqueueEmailAsync(EmailJob job);
    ValueTask<EmailJob> DequeueEmailAsync(CancellationToken cancellationToken);
}