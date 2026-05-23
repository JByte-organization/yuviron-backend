using System;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class UserSettings : Entity
{
    // --- Интерфейс и Внешний вид ---
    public string ThemeMode { get; private set; } = "system"; 
    public Guid? ThemeId { get; private set; } 
    public Guid? CustomThemeId { get; private set; } 

    // --- Плеер и Звук ---
    public int AudioQualityPreference { get; private set; } = 128; 
    public int CrossfadeMs { get; private set; } 

    // --- Приватность (Free) ---
    public bool MakePlaylistsPublicByDefault { get; private set; } = true;
    public bool ShowFollowers { get; private set; } = true;
    public bool ShowActivity { get; private set; } = true;

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
            ThemeMode = "system",
            AudioQualityPreference = 128, 
            CrossfadeMs = 0,
            MakePlaylistsPublicByDefault = true,
            ShowFollowers = true,
            ShowActivity = true,
            PrivateSession = false,
            UpdatedAt = utcNow
        };
    }

    public void UpdatePreferences(
        string themeMode, 
        int audioQuality, 
        int crossfadeMs,
        bool makePlaylistsPublic,
        bool showFollowers,
        bool showActivity,
        bool privateSession,
        DateTime utcNow)
    {
        ThemeMode = themeMode;
        AudioQualityPreference = audioQuality;
        CrossfadeMs = crossfadeMs;
        MakePlaylistsPublicByDefault = makePlaylistsPublic;
        ShowFollowers = showFollowers;
        ShowActivity = showActivity;
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