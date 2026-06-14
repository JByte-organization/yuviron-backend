using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface ISystemContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.OutboxMessage> OutboxMessages { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ExternalMapping> ExternalMappings { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.FileMetadata> FileMetadata { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ListeningEvent> ListeningEvents { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.TrackListenHeatmap> TrackListenHeatmaps { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Notification> Notifications { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.ReleaseNotificationTemplate> ReleaseNotificationTemplates { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Achievement> Achievements { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserAchievement> UserAchievements { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserAchievementProgress> UserAchievementProgress { get; }
}