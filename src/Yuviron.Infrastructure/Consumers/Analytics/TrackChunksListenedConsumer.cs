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

public class TrackChunksListenedConsumer : IConsumer<TrackChunksListenedEvent>
{
    private readonly string _connectionString;
    private readonly ILogger<TrackChunksListenedConsumer> _logger;

    public TrackChunksListenedConsumer(IConfiguration configuration, ILogger<TrackChunksListenedConsumer> logger)
    {
        _connectionString = ClickHouseConnectionStringFactory.Build(configuration);
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrackChunksListenedEvent> context)
    {
        var msg = context.Message;
        
        if (msg.StartSeconds == null || msg.StartSeconds.Count == 0) return;

        try
        {
            using var connection = new ClickHouseConnection(_connectionString);
            await connection.OpenAsync(context.CancellationToken);

            using var bulkCopy = new ClickHouseBulkCopy(connection)
            {
                DestinationTableName = "yuviron_analytics.listening_chunks",
                BatchSize = 1000
            };

            var table = new DataTable();
            table.Columns.Add("TrackId", typeof(Guid));
            table.Columns.Add("UserId", typeof(Guid));
            table.Columns.Add("PlayedAt", typeof(DateTime));
            table.Columns.Add("StartSecond", typeof(ushort));
            table.Columns.Add("EndSecond", typeof(ushort));
            table.Columns.Add("CountryCode", typeof(string));
            table.Columns.Add("DeviceType", typeof(string));

            for (int i = 0; i < msg.StartSeconds.Count; i++)
            {
                if (msg.StartSeconds[i] < 0 || msg.EndSeconds[i] < 0) continue;

                table.Rows.Add(
                    msg.TrackId,
                    msg.UserId == Guid.Empty ? DBNull.Value : msg.UserId,
                    msg.PlayedAt,
                    (ushort)msg.StartSeconds[i],
                    (ushort)msg.EndSeconds[i],
                    msg.CountryCode ?? "Unknown",
                    msg.DeviceType
                );
            }

            if (table.Rows.Count > 0)
            {
                await bulkCopy.InitAsync();
                await bulkCopy.WriteToServerAsync(table, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write analytics chunks to ClickHouse for Track: {TrackId}", msg.TrackId);
            throw; 
        }
    }
}