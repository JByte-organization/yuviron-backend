using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Commands.BlockUser;

public sealed record BlockUserCommand(
    Guid UserId,
    BlockType BlockType,
    string ReasonCode,
    string Description,
    DateTime? EndsAt
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}