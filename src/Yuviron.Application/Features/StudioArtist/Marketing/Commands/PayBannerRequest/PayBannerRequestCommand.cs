using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Commands.PayBannerRequest;

public record PayBannerResponse(string CheckoutUrl);

public sealed record PayBannerRequestCommand(
    Guid RequestId,
    Guid ArtistId,
    string SuccessUrl,
    string CancelUrl
) : IRequest<PayBannerResponse>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
