using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;

public sealed record CreateMoodCommand(
    string Name,
    string? CoverUrl
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}