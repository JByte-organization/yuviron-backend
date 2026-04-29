namespace Yuviron.Application.Features.Client.Home.Queries.GetHomeBanners;

public record HomeBannerDto(
    Guid Id,
    string Title,
    string BannerUrl,
    string TargetUrl
);