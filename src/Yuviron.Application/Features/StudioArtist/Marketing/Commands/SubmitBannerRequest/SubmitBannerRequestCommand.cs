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
    int DurationDays,
    string? TargetCountries,
    string? TargetGenres,
    string SuccessUrl,
    string CancelUrl   
) : IRequest<SubmitBannerResponse>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
