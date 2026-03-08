using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Domain.Common;
using Yuviron.Infrastructure.Persistence; 

namespace Yuviron.Infrastructure.BackgroundJobs;

public class ProcessOutboxMessagesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;
    
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
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                var strategy = dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                    var now = DateTime.UtcNow;

                    var messages = await dbContext.OutboxMessages
                        .FromSqlInterpolated($@"
                            SELECT * FROM outbox_messages 
                            WHERE ProcessedOnUtc IS NULL 
                              AND NextAttemptUtc <= {now} 
                            ORDER BY NextAttemptUtc 
                            LIMIT 20 
                            FOR UPDATE SKIP LOCKED")
                        .ToListAsync(stoppingToken);

                    messagesProcessedInBatch = messages.Count;

                    if (messages.Any())
                    {
                        foreach (var message in messages)
                        {
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

                                await publisher.Publish(domainEvent, stoppingToken);
                                
                                message.MarkAsProcessed(DateTime.UtcNow);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in Outbox Worker.");
            }

            if (messagesProcessedInBatch == 20)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}