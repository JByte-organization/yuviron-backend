using ClickHouse.Client.ADO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.Analytics;

public class ClickHouseInitializer : IHostedService
{
    private readonly string _connectionString;
    private readonly ILogger<ClickHouseInitializer> _logger;

    public ClickHouseInitializer(IConfiguration configuration, ILogger<ClickHouseInitializer> logger)
    {
        var rawConn = configuration.GetConnectionString("ClickHouse") 
            ?? throw new InvalidOperationException("ClickHouse connection string missing.");
            
        _connectionString = rawConn.Replace("Database=yuviron_analytics;", ""); 
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var connection = new ClickHouseConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            
            command.CommandText = "CREATE DATABASE IF NOT EXISTS yuviron_analytics";
            await command.ExecuteNonQueryAsync(cancellationToken);

            command.CommandText = "USE yuviron_analytics";
            await command.ExecuteNonQueryAsync(cancellationToken);

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS listening_chunks (
                    TrackId UUID,
                    UserId Nullable(UUID),
                    PlayedAt DateTime,
                    StartSecond UInt16,
                    EndSecond UInt16,
                    CountryCode String,
                    DeviceType String
                ) ENGINE = MergeTree()
                ORDER BY (TrackId, PlayedAt)
                PARTITION BY toYYYYMM(PlayedAt);";

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("ClickHouse database and tables initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ClickHouse database.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}