using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class UserSettings : Entity
{

    public string LanguageCode { get; private set; } = "en";
    public string ThemeMode { get; private set; } = "system"; 
    public Guid? CustomThemeId { get; private set; }
    public int AudioQualityPreference { get; private set; } = 320;
    public int CrossfadeMs { get; private set; }
    public bool PipEnabled { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual CustomTheme? CustomTheme { get; private set; }

    private UserSettings() { }

    public static UserSettings Create(Guid userId, string languageCode, DateTime utcNow)
    {
        return new UserSettings
        {
            Id = userId,
            LanguageCode = languageCode,
            ThemeMode = "system",
            AudioQualityPreference = 320, 
            CrossfadeMs = 0,
            PipEnabled = false,
            UpdatedAt = utcNow
        };
    }

    public void UpdatePreferences(string languageCode, string themeMode, int audioQuality, int crossfadeMs, bool pipEnabled, DateTime utcNow)
    {
        LanguageCode = languageCode;
        ThemeMode = themeMode;
        AudioQualityPreference = audioQuality;
        CrossfadeMs = crossfadeMs;
        PipEnabled = pipEnabled;
        UpdatedAt = utcNow;
    }

    public void ApplyCustomTheme(Guid? customThemeId, DateTime utcNow)
    {
        CustomThemeId = customThemeId;
        UpdatedAt = utcNow;
    }
}