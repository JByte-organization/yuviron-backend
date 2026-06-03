using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetPayoutSettings;

public sealed record GetPayoutSettingsQuery(Guid ArtistId) : IRequest<PayoutSettingsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}