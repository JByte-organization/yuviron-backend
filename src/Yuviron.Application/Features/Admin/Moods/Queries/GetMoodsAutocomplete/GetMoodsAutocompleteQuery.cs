using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoodsAutocomplete;

public sealed record GetMoodsAutocompleteQuery(
    string SearchTerm, 
    int Limit = 10
) : IRequest<List<MoodAutocompleteDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}