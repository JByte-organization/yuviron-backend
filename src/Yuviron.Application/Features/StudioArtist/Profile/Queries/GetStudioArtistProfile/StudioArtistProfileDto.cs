using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.StudioArtist.Profile.Queries.GetStudioArtistProfile;

public sealed record StudioArtistProfileDto(
    Guid Id,
    string Name,
    string VerificationStatus,
    bool IsPremium,
    StudioArtistDetailsDto Details,
    ProfileStatsDto Stats,
    StudioArtistFinanceDto Finance,
    List<StudioTeamMemberDto> Team,
    List<StudioSocialLinkDto> SocialLinks
);

public sealed record StudioArtistDetailsDto(
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    DateTime CreatedAt
);

public sealed record ProfileStatsDto(
    long TotalPlays,
    int MonthlyListenersCount
);

public sealed record StudioArtistFinanceDto(
    decimal AvailableBalance,
    decimal HeldBalance,
    decimal TotalEarned,
    StudioPayoutSettingsDto? PayoutSettings
);

public sealed record StudioPayoutSettingsDto(
    string Method,
    string AccountDetails,
    decimal MinWithdrawAmount,
    int PlatformPercent
);

public sealed record StudioTeamMemberDto(
    Guid UserId,
    string Email,
    string FirstName,
    string Role,
    DateTime AddedAt
);

public sealed record StudioSocialLinkDto(
    string Type, 
    string Url
);