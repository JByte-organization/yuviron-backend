using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Moods.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoodById;

public sealed record GetMoodByIdQuery(Guid Id) : IRequest<GetMoodByIdDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}