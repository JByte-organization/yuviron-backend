using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    Guid? AvatarFileId, // <-- GUID
    Guid? BannerFileId, // <-- GUID
    AccountState AccountState,
    bool AcceptMarketing,
    string FirstName,
    DateTime DateOfBirth,
    Gender Gender,
    IReadOnlyCollection<Guid>? RoleIds = null
) : IRequest<Unit>, ISecuredRequest, ISensitiveRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageUsers;
}



