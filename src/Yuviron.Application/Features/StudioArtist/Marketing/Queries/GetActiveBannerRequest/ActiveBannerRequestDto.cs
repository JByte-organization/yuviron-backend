using Yuviron.Domain.Enums;
using System;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Queries.GetActiveBannerRequest;

public record ActiveBannerRequestDto(
    Guid Id,
    Guid ArtistId,
    Guid? AlbumId,
    string Title,
    string? BannerUrl,
    BannerRequestStatus Status,
    string? AdminNotes,
    bool IsPaid,
    int DurationDays,
    string? TargetCountries,
    string? TargetGenres,
    DateTime? EndsAtUtc,
    DateTime CreatedAt
);
