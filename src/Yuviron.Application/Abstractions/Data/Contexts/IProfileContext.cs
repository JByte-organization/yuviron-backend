using Yuviron.Application.Abstractions.Data;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Data.Contexts;

public interface IProfileContext : IDataContext
{
    IQueryable<UserProfile> UserProfiles { get; }
    IQueryable<Theme> Themes { get; }
    IQueryable<CustomTheme> CustomThemes { get; }
    IQueryable<UserSettings> UserSettings { get; }
    IQueryable<UserNotificationPreference> UserNotificationPreferences { get; }
}