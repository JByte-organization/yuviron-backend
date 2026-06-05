using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Commands.SubmitBannerRequest;

public record SubmitBannerResponse(Guid RequestId, string CheckoutUrl);

public sealed record SubmitBannerRequestCommand(
    Guid ArtistId,
    Guid AlbumId, 
    string Title,
    Guid BannerFileId,
    string SuccessUrl,
    string CancelUrl   
) : IRequest<SubmitBannerResponse>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}