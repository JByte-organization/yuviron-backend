using Microsoft.Extensions.Configuration;

namespace Yuviron.Infrastructure.Analytics;

/// <summary>
/// Builds ClickHouse ADO.NET connection strings from the discrete "ClickHouse:*" settings
/// (Host, HttpPort, Database, User, Password) that infra already injects into containers
/// as ClickHouse__Host / ClickHouse__HttpPort / etc. — there is no "ConnectionStrings:ClickHouse" entry.
/// </summary>
internal static class ClickHouseConnectionStringFactory
{
    public static string Build(IConfiguration configuration, bool includeDatabase = true)
    {
        var section = configuration.GetSection("ClickHouse");
        var host = section["Host"]
            ?? throw new InvalidOperationException("ClickHouse:Host is missing.");
        var port = section["HttpPort"] ?? "8123";
        var user = section["User"]
            ?? throw new InvalidOperationException("ClickHouse:User is missing.");
        var password = section["Password"]
            ?? throw new InvalidOperationException("ClickHouse:Password is missing.");

        var connectionString = $"Host={host};Port={port};Protocol=http;Username={user};Password={password}";
        if (!includeDatabase)
        {
            return connectionString;
        }

        var database = section["Database"]
            ?? throw new InvalidOperationException("ClickHouse:Database is missing.");
        return $"{connectionString};Database={database}";
    }
}
