using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

public sealed record CreateGenreCommand(
    string Name,
    string? CoverUrl
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}