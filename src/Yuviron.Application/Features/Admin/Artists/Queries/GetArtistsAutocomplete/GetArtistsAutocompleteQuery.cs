using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistsAutocomplete;

public sealed record GetArtistsAutocompleteQuery(
    string SearchTerm, 
    int Limit = 10
) : IRequest<List<ArtistAutocompleteDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}