using Yuviron.Application.Abstractions.Data;
namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IProfileContext : IDataContext
{
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserProfile> UserProfiles { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.Theme> Themes { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.CustomTheme> CustomThemes { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserSettings> UserSettings { get; }
    System.Linq.IQueryable<Yuviron.Domain.Entities.UserNotificationPreference> UserNotificationPreferences { get; }
}