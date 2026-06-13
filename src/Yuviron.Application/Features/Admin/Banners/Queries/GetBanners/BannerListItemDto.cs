using System;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

public record BannerListItemDto(
    Guid Id,
    string Title,
    string BannerUrl,
    bool IsActive,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc
);
