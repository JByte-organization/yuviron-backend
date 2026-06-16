using System.Diagnostics;
using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Infrastructure.Persistence; 

namespace Yuviron.Infrastructure.BackgroundJobs;

public class ProcessOutboxMessagesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;
    
    private static readonly ActivitySource ActivitySource = new("Yuviron.Infrastructure");
    
    private const int MaxRetries = 5; 

    public ProcessOutboxMessagesJob(IServiceProvider serviceProvider, ILogger<ProcessOutboxMessagesJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int messagesProcessedInBatch = 0;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var strategy = dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                    var now = DateTime.UtcNow;

                    List<OutboxMessage> messages;

                    using (SuppressInstrumentationScope.Begin())
                    {
                        messages = await ((DbSet<OutboxMessage>)dbContext.OutboxMessages)
                            .FromSqlInterpolated($@"
                                SELECT * FROM outbox_messages 
                                WHERE ProcessedOnUtc IS NULL 
                                  AND NextAttemptUtc <= {now} 
                                ORDER BY NextAttemptUtc 
                                LIMIT 20 
                                FOR UPDATE SKIP LOCKED")
                            .ToListAsync(stoppingToken);
                    }

                    messagesProcessedInBatch = messages.Count;

                    if (messages.Any())
                    {
                        foreach (var message in messages)
                        {
                            using var activity = ActivitySource.StartActivity("ProcessOutboxMessage", ActivityKind.Consumer);
                            if (activity != null && !string.IsNullOrEmpty(message.TraceId))
                            {
                                activity.SetParentId(message.TraceId);
                            }

                            try
                            {
                                var eventType = Type.GetType(message.Type);
                                if (eventType == null) 
                                {
                                    throw new InvalidOperationException($"Type not found: {message.Type}");
                                }

                                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                                if (domainEvent == null) 
                                {
                                    throw new InvalidOperationException($"Deserialization returned null for {message.Type}");
                                }

                                Console.WriteLine($"[Outbox-Debug] Publishing message {message.Id} of type {message.Type}");
                                await publishEndpoint.Publish(domainEvent, eventType, stoppingToken);
                                
                                message.MarkAsProcessed(DateTime.UtcNow);
                                Console.WriteLine($"[Outbox-Debug] Successfully processed message {message.Id}");
                            }
                            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to process outbox message {MessageId}. Attempt {Retry}", message.Id, message.RetryCount + 1);

                                if (message.RetryCount >= MaxRetries - 1)
                                {
                                    _logger.LogError("Message {Id} failed after {MaxRetries} attempts. Dead Letter.", message.Id, MaxRetries);
                                    message.MarkAsFailed(ex.Message, null);
                                }
                                else
                                {
                                    var delaySeconds = Math.Pow(2, message.RetryCount) * 5;
                                    var nextAttempt = DateTime.UtcNow.AddSeconds(delaySeconds);
                                    message.MarkAsFailed(ex.Message, nextAttempt);
                                }
                            }
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken); 
                    }
                });
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in Outbox Worker.");
            }

            var delay = messagesProcessedInBatch == 20
                ? TimeSpan.FromMilliseconds(100)
                : TimeSpan.FromSeconds(5);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
