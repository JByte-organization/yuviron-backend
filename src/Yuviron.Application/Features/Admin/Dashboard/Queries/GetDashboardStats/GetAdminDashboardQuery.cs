using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

public sealed record GetAdminDashboardQuery : IRequest<AdminDashboardDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}