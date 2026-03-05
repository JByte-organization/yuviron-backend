using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageUsers;
}