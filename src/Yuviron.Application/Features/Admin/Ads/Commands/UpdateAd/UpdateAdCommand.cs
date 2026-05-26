using MediatR;

namespace Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;

public sealed record UpdateAdRequest(
    string AdvertiserName, 
    string Title, 
    string? ClickUrl
);

public sealed record UpdateAdCommand(
    Guid AdId, 
    string AdvertiserName, 
    string Title, 
    string? ClickUrl
) : IRequest;