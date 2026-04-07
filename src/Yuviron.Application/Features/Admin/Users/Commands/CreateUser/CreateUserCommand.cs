using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

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




