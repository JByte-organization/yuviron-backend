using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}




