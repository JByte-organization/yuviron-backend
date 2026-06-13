using System;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerById;

public record BannerDetailsDto(
    Guid Id,
    Guid? ArtistId,
    string? ArtistName,
    string Title,
    string BannerUrl,
    string TargetUrl,
    bool IsActive,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    string? TargetCountries,
    string? TargetGenres,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
