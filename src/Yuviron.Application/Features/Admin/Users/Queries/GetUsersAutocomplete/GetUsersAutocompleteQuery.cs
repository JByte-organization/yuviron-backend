using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsersAutocomplete;

public sealed record GetUsersAutocompleteQuery(
    string SearchTerm, 
    int Limit = 10,
    AppPermission? RequiredPermission = null
) : IRequest<List<UserAutocompleteDto>>, ISecuredRequest
{
    AppPermission ISecuredRequest.RequiredPermission => AppPermission.ManageCatalog; 
}
