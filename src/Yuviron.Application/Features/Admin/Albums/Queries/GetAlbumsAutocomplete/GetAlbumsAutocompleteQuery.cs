using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Queries.GetAlbumsAutocomplete;

public sealed record GetAlbumsAutocompleteQuery(
    string SearchTerm, 
    int Limit = 10
) : IRequest<List<AlbumAutocompleteDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}