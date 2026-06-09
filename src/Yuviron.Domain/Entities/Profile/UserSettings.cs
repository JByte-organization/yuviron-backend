using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class UserSettings : Entity
{
    // --- Интерфейс и Внешний вид ---
    public string ThemeMode { get; private set; } = Yuviron.Domain.Enums.ThemeMode.System.ToString();
    public Guid? ThemeId { get; private set; }
    public Guid? CustomThemeId { get; private set; }

    // --- Плеер и Звук ---
    public int AudioQualityPreference { get; private set; } = 128;
    public int CrossfadeMs { get; private set; }

    // --- Приватность (Free) ---
    public bool MakePlaylistsPublicByDefault { get; private set; } = true;
    public bool ShowFollowers { get; private set; } = true;

    // --- Приватность (Premium) ---
    public bool PrivateSession { get; private set; } = false;

    public DateTime UpdatedAt { get; private set; }

    // --- Навигационные свойства ---
    public virtual User User { get; private set; } = null!;
    public virtual Theme? Theme { get; private set; }
    public virtual CustomTheme? CustomTheme { get; private set; }

    private UserSettings() { }

    public static UserSettings Create(Guid userId, DateTime utcNow)
    {
        return new UserSettings
        {
            Id = userId,
            ThemeMode = Yuviron.Domain.Enums.ThemeMode.System.ToString(),
            AudioQualityPreference = 128,
            CrossfadeMs = 0,
            MakePlaylistsPublicByDefault = true,
            ShowFollowers = true,
            PrivateSession = false,
            UpdatedAt = utcNow
        };
    }

    public void UpdatePreferences(
        Yuviron.Domain.Enums.ThemeMode themeMode,
        int audioQuality,
        int crossfadeMs,
        bool makePlaylistsPublic,
        bool showFollowers,
        bool privateSession,
        DateTime utcNow)
    {
        ThemeMode = themeMode.ToString();
        AudioQualityPreference = audioQuality;
        CrossfadeMs = crossfadeMs;
        MakePlaylistsPublicByDefault = makePlaylistsPublic;
        ShowFollowers = showFollowers;
        PrivateSession = privateSession;
        UpdatedAt = utcNow;
    }

    public void UpdateTheme(Yuviron.Domain.Enums.ThemeMode themeMode, DateTime utcNow)
    {
        ThemeMode = themeMode.ToString();
        UpdatedAt = utcNow;
    }

    public void UpdateAudioQuality(int audioQuality, DateTime utcNow)
    {
        AudioQualityPreference = audioQuality;
        UpdatedAt = utcNow;
    }

    public void UpdateCrossfade(int crossfadeMs, DateTime utcNow)
    {
        CrossfadeMs = crossfadeMs;
        UpdatedAt = utcNow;
    }

    public void UpdatePrivacy(bool makePlaylistsPublic, bool showFollowers, DateTime utcNow)
    {
        MakePlaylistsPublicByDefault = makePlaylistsPublic;
        ShowFollowers = showFollowers;
        UpdatedAt = utcNow;
    }

    public void TogglePrivateSession(bool privateSession, DateTime utcNow)
    {
        PrivateSession = privateSession;
        UpdatedAt = utcNow;
    }

    public void ApplyDesign(Guid? themeId, Guid? customThemeId, DateTime utcNow)
    {
        ThemeId = themeId;
        CustomThemeId = customThemeId;
        UpdatedAt = utcNow;
    }
}
