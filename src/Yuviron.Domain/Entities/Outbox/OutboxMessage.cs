namespace Yuviron.Domain.Entities;


public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextAttemptUtc { get; private set; }
    
    private OutboxMessage() { }
    
    public static OutboxMessage Create(string type, string content, DateTime occurredOnUtc)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            OccurredOnUtc = occurredOnUtc,
            RetryCount = 0,
            NextAttemptUtc = occurredOnUtc
        };
    }

    
    public void MarkAsProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
    }
    
    public void MarkAsFailed(string error, DateTime? nextAttemptUtc)
    {
        Error = error;
        RetryCount++;
        NextAttemptUtc = nextAttemptUtc;
    }
}