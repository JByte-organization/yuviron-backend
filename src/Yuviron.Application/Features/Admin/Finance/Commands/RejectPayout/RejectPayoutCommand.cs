using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Commands.RejectPayout;

public sealed record RejectPayoutCommand(
    Guid PayoutRequestId,
    string Reason
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}