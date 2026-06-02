using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Dashboard.Queries.GetDashboardStats;

public sealed record GetStudioArtistDashboardQuery : IRequest<StudioArtistDashboardDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}
