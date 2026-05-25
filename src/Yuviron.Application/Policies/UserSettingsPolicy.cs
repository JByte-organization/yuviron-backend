using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Yuviron.Application.Configuration;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Policies;

public class UserSettingsPolicy 
{
    private readonly int _freeTierQuality;
    private readonly int _premiumMaxQuality;

    public UserSettingsPolicy(IOptions<AudioSettingsOptions> audioOptions)
    {
        var qualities = audioOptions.Value.HlsQualities;
        
        if (qualities == null || qualities.Length == 0)
        {
            _freeTierQuality = 128;
            _premiumMaxQuality = 320;
        }
        else
        {
            _freeTierQuality = qualities.Min();
            _premiumMaxQuality = qualities.Max();
        }
    }

    public int GetAllowedStreamQuality(UserSettings? settings, bool hasHighQualityPermission)
    {
        if (!hasHighQualityPermission) return _freeTierQuality;
        
        var preferred = settings?.AudioQualityPreference ?? _premiumMaxQuality;
        return Math.Min(preferred, _premiumMaxQuality);
    }

    public int SanitizeAudioQuality(bool hasHighQualityPermission, int requestedQuality)
    {
        if (!hasHighQualityPermission) return _freeTierQuality; 
        
        return requestedQuality > _premiumMaxQuality ? _premiumMaxQuality : requestedQuality;
    }

    public bool CanUsePrivateSession(bool hasPrivateSessionPermission, bool requestedPrivateSession) 
        => hasPrivateSessionPermission && requestedPrivateSession;

    public bool CanUseCustomTheme(bool hasCustomThemePermission) 
        => hasCustomThemePermission;

    public bool CanUsePresetTheme(bool hasCustomThemePermission, bool isThemePremiumOnly)
    {
        return !isThemePremiumOnly || hasCustomThemePermission; 
    }

    public bool CanUploadAnimatedMedia(bool hasAnimatedMediaPermission, string contentType)
    {
        bool isAnimatedFormat = contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase) ||
                                contentType.Equals("image/gif", StringComparison.OrdinalIgnoreCase);

        return !isAnimatedFormat || hasAnimatedMediaPermission; 
    }
}