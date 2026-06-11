using System;
using System.Collections.Generic;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries; 

public record ArtistOwnerDto(
    Guid UserId, 
    string Email, 
    string FirstName
);

public record AdminSocialLinkDto(SocialLinkType Type, string Url);
public record AdminArtistPinDto(ArtistPinType EntityType, Guid EntityId, int Position);

public record ArtistDetailsDto(
    Guid Id,
    ArtistOwnerDto? Owner,
    string Name,
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    VerificationStatus VerificationStatus,
    int TotalAlbums,   
    int TotalTracks,   
    List<AdminSocialLinkDto> SocialLinks, 
    List<AdminArtistPinDto> Pins,      
    DateTime CreatedAt,
    DateTime UpdatedAt
);