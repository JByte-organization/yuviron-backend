using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Finance.Commands.ApprovePayout;

public sealed record ApprovePayoutCommand(
    Guid PayoutRequestId,
    string? ProviderReferenceId 
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel; 
}