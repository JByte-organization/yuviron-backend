using MediatR;

namespace Yuviron.Application.Features.Admin.Analytics.Commands.SyncAnalytics;

public record SyncAnalyticsCommand(
    Dictionary<Guid, long> TracksToSync,
    Dictionary<Guid, long> ArtistsToSync) : IRequest;
