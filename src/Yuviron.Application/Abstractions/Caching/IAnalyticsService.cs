namespace Yuviron.Application.Abstractions.Services;

public interface IAnalyticsService
{
    Task RecordTrackPlayAsync(Guid trackId, Guid artistId, CancellationToken ct = default);
}