using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;
public sealed record CreateGenreCommand(
    string Name,
    string? CoverUrl
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}