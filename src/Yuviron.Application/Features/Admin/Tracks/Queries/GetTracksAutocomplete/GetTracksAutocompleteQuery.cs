using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracksAutocomplete;

public sealed record GetTracksAutocompleteQuery(
    string SearchTerm, 
    int Limit = 10
) : IRequest<List<TrackAutocompleteDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}