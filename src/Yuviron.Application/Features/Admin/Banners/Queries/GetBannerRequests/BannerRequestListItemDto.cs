using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequests;

public record BannerRequestListItemDto(
    Guid Id,
    Guid ArtistId,
    string ArtistName,
    Guid? AlbumId,
    string? AlbumTitle,
    string Title,
    string? BannerUrl,
    BannerRequestStatus Status,
    string? AdminNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);