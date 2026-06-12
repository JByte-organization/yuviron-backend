using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Analytics;

namespace Yuviron.Infrastructure.Consumers.Analytics;

public class AdImpressionRecordedConsumer : IConsumer<AdImpressionRecordedEvent>
{
    private readonly string _connectionString;
    private readonly ILogger<AdImpressionRecordedConsumer> _logger;

    public AdImpressionRecordedConsumer(IConfiguration configuration, ILogger<AdImpressionRecordedConsumer> logger)
    {
        _connectionString = ClickHouseConnectionStringFactory.Build(configuration);
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AdImpressionRecordedEvent> context)
    {
        var msg = context.Message;

        try
        {
            using var connection = new ClickHouseConnection(_connectionString);
            await connection.OpenAsync(context.CancellationToken);

            using var bulkCopy = new ClickHouseBulkCopy(connection)
            {
                DestinationTableName = "yuviron_analytics.ad_impressions_log",
                BatchSize = 100
            };

            var table = new DataTable();
            table.Columns.Add("AdId", typeof(Guid));
            table.Columns.Add("UserId", typeof(Guid));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Context", typeof(string));
            table.Columns.Add("IsClicked", typeof(byte));

            table.Rows.Add(
                msg.AdId,
                msg.UserId ?? Guid.Empty,
                msg.Timestamp,
                msg.Context ?? "unknown",
                (byte)(msg.IsClicked ? 1 : 0)
            );

            await bulkCopy.InitAsync();
            await bulkCopy.WriteToServerAsync(table, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write Ad impression to ClickHouse for Ad: {AdId}", msg.AdId);
            throw;
        }
    }
}
