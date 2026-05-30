using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Commands.ApproveRequest;

public sealed record ApproveVerificationRequestCommand(
    Guid RequestId,
    string? AdminNote
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}