using System;

namespace Yuviron.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    bool IsPremium,
    UserProfileDto Profile,
    UserSettingsDto Settings
);

public sealed record UserProfileDto(
    string FirstName,
    string? AvatarUrl,
    string? BannerUrl,
    string? Country,
    string? City,
    string? Bio,
    DateTime DateOfBirth,
    string Gender
);

public sealed record UserSettingsDto(
    string ThemeMode,
    Guid? ThemeId,
    Guid? CustomThemeId,
    int AudioQualityPreference,
    int CrossfadeMs,
    bool MakePlaylistsPublicByDefault,
    bool ShowFollowers,
    bool ShowActivity,
    bool PrivateSession
);