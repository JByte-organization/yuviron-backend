using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    DateTime DateOfBirth,
    Gender Gender,
    bool AcceptMarketing,
    bool AcceptTerms,
    AccountState AccountState,
    IReadOnlyCollection<Guid>? RoleIds = null
) : IRequest<Guid>, ISecuredRequest, ISensitiveRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageUsers;
}




