using System;

namespace Yuviron.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    bool IsPremium,
    CurrentUserProfileDto Profile,
    CurrentUserSettingsDto Settings
);

public sealed record CurrentUserProfileDto(
    string FirstName,
    string? AvatarUrl,
    string? BannerUrl,
    string? Country,
    string? City,
    string? Bio,
    DateTime DateOfBirth,
    string Gender
);

public sealed record CurrentUserSettingsDto(
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