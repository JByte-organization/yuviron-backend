using System;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Policies;

public class UserSettingsPolicy 
{
    private const int FreeTierQuality = 128;
    private const int PremiumMaxQuality = 320;

    public int GetAllowedStreamQuality(UserSettings? settings, bool hasHighQualityPermission)
    {
        if (!hasHighQualityPermission) return FreeTierQuality;
        return Math.Min(settings?.AudioQualityPreference ?? PremiumMaxQuality, PremiumMaxQuality);
    }

    public int SanitizeAudioQuality(bool hasHighQualityPermission, int requestedQuality)
    {
        if (!hasHighQualityPermission) return FreeTierQuality; 
        return requestedQuality > PremiumMaxQuality ? PremiumMaxQuality : requestedQuality;
    }

    public bool CanUsePrivateSession(bool hasPrivateSessionPermission, bool requestedPrivateSession) 
        => hasPrivateSessionPermission && requestedPrivateSession;

    public bool CanUseCustomTheme(bool hasCustomThemePermission) 
        => hasCustomThemePermission;

    public bool CanUsePresetTheme(bool hasCustomThemePermission, bool isThemePremiumOnly)
    {
        return !isThemePremiumOnly || hasCustomThemePermission; 
    }

    public bool CanUploadAnimatedMedia(bool hasAnimatedMediaPermission, string fileExtension)
    {
        bool isAnimated = fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                          fileExtension.Equals(".webm", StringComparison.OrdinalIgnoreCase);

        return !isAnimated || hasAnimatedMediaPermission; 
    }
}