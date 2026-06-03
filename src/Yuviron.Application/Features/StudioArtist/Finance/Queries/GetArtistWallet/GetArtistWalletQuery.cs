using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistWallet;

public sealed record GetArtistWalletQuery(Guid ArtistId) : IRequest<ArtistWalletDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}