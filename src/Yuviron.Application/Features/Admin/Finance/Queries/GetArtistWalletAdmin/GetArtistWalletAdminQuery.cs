using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetArtistWalletAdmin;

public sealed record GetArtistWalletAdminQuery(Guid ArtistId) : IRequest<AdminArtistWalletDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}