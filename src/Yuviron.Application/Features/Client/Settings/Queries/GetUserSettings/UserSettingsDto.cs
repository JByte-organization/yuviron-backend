using System;

namespace Yuviron.Application.Features.Client.Settings.Queries.GetUserSettings;

public class UserSettingsDto
{
    public string ThemeMode { get; set; } = string.Empty;
    public Guid? ThemeId { get; set; }
    public Guid? CustomThemeId { get; set; }
    public int AudioQualityPreference { get; set; }
    public int CrossfadeMs { get; set; }
    public bool MakePlaylistsPublicByDefault { get; set; }
    public bool ShowFollowers { get; set; }
    public bool PrivateSession { get; set; }
}
