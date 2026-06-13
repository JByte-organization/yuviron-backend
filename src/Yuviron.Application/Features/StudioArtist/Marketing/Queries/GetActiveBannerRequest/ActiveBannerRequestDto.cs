using System;
using Yuviron.Domain.Enums;

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
    DateTime CreatedAt
);
