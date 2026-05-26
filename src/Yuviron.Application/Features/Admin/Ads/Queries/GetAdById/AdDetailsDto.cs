namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAdById;

public record AdDetailsDto(
    Guid Id,
    string AdvertiserName,
    string Title,
    string AudioUrl,
    string ImageUrl,
    string? ClickUrl,
    bool IsActive,
    int TotalImpressions,
    int TotalClicks,
    DateTime CreatedAt,
    DateTime UpdatedAt
);