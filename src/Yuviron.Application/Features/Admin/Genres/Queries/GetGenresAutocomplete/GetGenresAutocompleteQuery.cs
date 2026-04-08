using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Genres.Queries.GetGenresAutocomplete;

public sealed record GetGenresAutocompleteQuery(
    string SearchTerm, 
    int Limit = 10
) : IRequest<List<GenreAutocompleteDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}