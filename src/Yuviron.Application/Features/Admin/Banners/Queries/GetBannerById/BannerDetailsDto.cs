
namespace Yuviron.Application.Features.Admin.Banners.Queries.DTOs;

public record BannerDetailsDto(
    Guid Id,
    string Title,
    string BannerUrl,
    string TargetUrl,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);