using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequestById;

public record BannerRequestDetailsDto(
    Guid Id,
    Guid ArtistId,
    string ArtistName,
    Guid SubmittedByUserId,
    string SubmittedByUserName,
    Guid? AlbumId,
    string? AlbumTitle,
    string Title,
    string? BannerUrl,
    BannerRequestStatus Status,
    bool IsPaid,
    string? StripePaymentIntentId,
    string? AdminNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
