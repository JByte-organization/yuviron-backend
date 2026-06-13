using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequestById;

public record BannerRequestDetailsDto(
    Guid Id,
    Guid ArtistId,
    string ArtistName,
    Guid? AlbumId,
    string? AlbumTitle,
    string Title,
    string? BannerUrl,
    BannerRequestStatus Status,
    string? AdminNotes,
    bool IsPaid,
    int DurationDays,
    string? TargetCountries,
    string? TargetGenres,
    DateTime? EndsAtUtc,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
