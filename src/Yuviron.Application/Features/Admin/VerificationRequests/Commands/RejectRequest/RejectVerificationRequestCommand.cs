using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Commands.RejectRequest;

public sealed record RejectVerificationRequestCommand(
    Guid RequestId,
    string? AdminNote
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}