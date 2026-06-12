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
        // С Database в строке подключения: yuviron_api_user не имеет грантов
        // CREATE DATABASE (см. 01-init.sh — только SELECT/INSERT/CREATE TABLE
        // на yuviron_analytics.*), поэтому база должна быть создана заранее
        // инфраструктурой, а не этим инициализатором.
        _connectionString = ClickHouseConnectionStringFactory.Build(configuration);
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var connection = new ClickHouseConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();

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

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ad_impressions_log (
                    AdId UUID,
                    UserId UUID,
                    Timestamp DateTime,
                    Context String,
                    IsClicked UInt8
                ) ENGINE = MergeTree()
                ORDER BY (AdId, Timestamp)
                PARTITION BY toYYYYMM(Timestamp);";
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