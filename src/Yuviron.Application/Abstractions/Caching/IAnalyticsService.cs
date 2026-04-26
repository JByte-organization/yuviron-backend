
namespace Yuviron.Application.Abstractions.Services;

public interface IAnalyticsService
{
    Task<Guid> StartPlaySessionAsync(Guid trackId, Guid userId, CancellationToken ct = default);
    Task CommitPlaySessionAsync(Guid sessionId, Guid trackId, Guid artistId, Guid userId, CancellationToken ct = default);
}