using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Genres.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenresById;

public sealed record GetGenreByIdQuery(Guid GenreId) : IRequest<GenreDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}