using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface ISystemContext : IDataContext
{
    IQueryable<OutboxMessage> OutboxMessages { get; }
    IQueryable<ExternalMapping> ExternalMappings { get; }
    IQueryable<FileMetadata> FileMetadata { get; }
    IQueryable<ListeningEvent> ListeningEvents { get; }
    IQueryable<TrackListenHeatmap> TrackListenHeatmaps { get; }
    IQueryable<Notification> Notifications { get; }
    IQueryable<ReleaseNotificationTemplate> ReleaseNotificationTemplates { get; }
    IQueryable<Achievement> Achievements { get; }
    IQueryable<UserAchievement> UserAchievements { get; }
    IQueryable<UserAchievementProgress> UserAchievementProgress { get; }
}