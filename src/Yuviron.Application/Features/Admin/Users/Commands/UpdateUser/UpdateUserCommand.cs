using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    string? AvatarUrl,
    AccountState AccountState,
    bool AcceptMarketing,
    string DisplayName,
    DateTime DateOfBirth,
    Gender Gender,
    IReadOnlyCollection<Guid>? RoleIds = null
) : IRequest<Unit>, ISecuredRequest, ISensitiveRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageUsers;
}




