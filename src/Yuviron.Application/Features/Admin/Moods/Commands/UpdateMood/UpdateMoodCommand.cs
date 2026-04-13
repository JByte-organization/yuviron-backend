using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;

public sealed record UpdateMoodCommand(
    Guid Id,
    string Name,
    string? CoverUrl
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}