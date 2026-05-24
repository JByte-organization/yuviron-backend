using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed record UpdateGenreCommand(
    Guid GenreId,
    string Name,
    Guid? CoverFileId // <-- GUID
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}