using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Queries.GetPayoutRequestDetails;

public sealed record GetPayoutRequestDetailsQuery(Guid PayoutRequestId) : IRequest<PayoutRequestDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel; 
}