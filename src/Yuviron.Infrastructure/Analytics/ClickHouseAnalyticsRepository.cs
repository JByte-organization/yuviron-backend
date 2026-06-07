using ClickHouse.Client.ADO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClickHouse.Client.Utility;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackRetention;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

namespace Yuviron.Infrastructure.Analytics;

public class ClickHouseAnalyticsRepository : IAnalyticsRepository
{
    private readonly string _connectionString;

    public ClickHouseAnalyticsRepository(IConfiguration configuration)
    {
        _connectionString = ClickHouseConnectionStringFactory.Build(configuration);
    }

    public async Task<List<TrackRetentionPointDto>> GetTrackRetentionAsync(Guid trackId, CancellationToken ct)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                arrayJoin(range(StartSecond, EndSecond)) AS SecondIndex, 
                CAST(COUNT() AS Int32) AS Plays
            FROM yuviron_analytics.listening_chunks
            WHERE TrackId = {trackId:UUID}
            GROUP BY SecondIndex
            ORDER BY SecondIndex ASC
            LIMIT 600;";
        
        command.AddParameter("trackId", trackId);

        var data = new List<(int Second, int Plays)>();
        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            data.Add((reader.GetInt32(0), reader.GetInt32(1)));
        }

        if (!data.Any()) return new List<TrackRetentionPointDto>();

        double maxPlays = data.Max(x => x.Plays);
        return data.Select(x => new TrackRetentionPointDto(
            x.Second, 
            x.Plays, 
            Math.Round((x.Plays / maxPlays) * 100.0, 2)
        )).ToList();
    }

    public async Task<List<PlaysOverTimePointDto>> GetTrackPlaysOverTimeAsync(Guid trackId, DateTime minDate, CancellationToken ct)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                toDate(PlayedAt) AS Date,
                CAST(COUNT() AS Int32) AS TotalPlays,
                CAST(uniqExact(UserId) AS Int32) AS UniqueListeners
            FROM yuviron_analytics.listening_chunks
            WHERE TrackId = {trackId:UUID} AND PlayedAt >= {minDate:DateTime}
            GROUP BY Date
            ORDER BY Date ASC;";
            
        command.AddParameter("trackId", trackId);
        command.AddParameter("minDate", minDate);

        var result = new List<PlaysOverTimePointDto>();

        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new PlaysOverTimePointDto(
                Date: reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                TotalPlays: reader.GetInt32(1),
                UniqueListeners: reader.GetInt32(2)
            ));
        }

        return result;
    }
    
    public async Task<List<PlaysOverTimePointDto>> GetArtistPlaysOverTimeAsync(IEnumerable<Guid> trackIds, DateTime minDate, CancellationToken ct)
    {
        if (!trackIds.Any()) return new List<PlaysOverTimePointDto>();
        
        // Безопасный способ передачи массива GUID в ClickHouse
        var idsString = string.Join(",", trackIds.Select(id => $"'{id}'"));

        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT 
                toDate(PlayedAt) AS Date,
                CAST(COUNT() AS Int32) AS TotalPlays,
                CAST(uniqExact(UserId) AS Int32) AS UniqueListeners
            FROM yuviron_analytics.listening_chunks
            WHERE TrackId IN ({idsString}) AND PlayedAt >= {{minDate:DateTime}}
            GROUP BY Date
            ORDER BY Date ASC;";
            
        command.AddParameter("minDate", minDate);

        var result = new List<PlaysOverTimePointDto>();
        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new PlaysOverTimePointDto(
                Date: reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                TotalPlays: reader.GetInt32(1),
                UniqueListeners: reader.GetInt32(2)
            ));
        }

        return result;
    }

    public async Task<List<AudienceGeographyDto>> GetArtistGeographyAsync(IEnumerable<Guid> trackIds, DateTime minDate, CancellationToken ct)
    {
        if (!trackIds.Any()) return new List<AudienceGeographyDto>();
        var idsString = string.Join(",", trackIds.Select(id => $"'{id}'"));

        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = connection.CreateCommand();
        // Считаем топ-10 стран по уникальным слушателям
        command.CommandText = $@"
            SELECT 
                CountryCode,
                CAST(uniqExact(UserId) AS Int32) AS Listeners
            FROM yuviron_analytics.listening_chunks
            WHERE TrackId IN ({idsString}) AND PlayedAt >= {{minDate:DateTime}} AND CountryCode != 'Unknown'
            GROUP BY CountryCode
            ORDER BY Listeners DESC
            LIMIT 10;";
            
        command.AddParameter("minDate", minDate);

        var result = new List<AudienceGeographyDto>();
        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new AudienceGeographyDto(reader.GetString(0), reader.GetInt32(1)));
        }

        return result;
    }

    public async Task<List<AudienceDeviceDto>> GetArtistDevicesAsync(IEnumerable<Guid> trackIds, DateTime minDate, CancellationToken ct)
    {
        if (!trackIds.Any()) return new List<AudienceDeviceDto>();
        var idsString = string.Join(",", trackIds.Select(id => $"'{id}'"));

        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(ct);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT 
                DeviceType,
                CAST(COUNT() AS Int32) AS Plays
            FROM yuviron_analytics.listening_chunks
            WHERE TrackId IN ({idsString}) AND PlayedAt >= {{minDate:DateTime}}
            GROUP BY DeviceType
            ORDER BY Plays DESC;";
            
        command.AddParameter("minDate", minDate);

        var result = new List<AudienceDeviceDto>();
        using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new AudienceDeviceDto(reader.GetString(0), reader.GetInt32(1)));
        }

        return result;
    }
}