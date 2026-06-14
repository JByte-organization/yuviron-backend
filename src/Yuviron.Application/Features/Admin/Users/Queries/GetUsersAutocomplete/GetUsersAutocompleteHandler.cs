using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsersAutocomplete;

public sealed class GetUsersAutocompleteHandler : IRequestHandler<GetUsersAutocompleteQuery, List<UserAutocompleteDto>>
{
    private readonly IIdentityContext _identityContext;

    public GetUsersAutocompleteHandler(IIdentityContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<List<UserAutocompleteDto>> Handle(GetUsersAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return new List<UserAutocompleteDto>();
        }

        var searchTerm = request.SearchTerm.Trim();

        var users = await _identityContext.Users
            .AsNoTracking()
            .WhereHasPermission(request.RequiredPermission)
            .Where(u => !u.IsDeleted && 
                        (u.Email.Contains(searchTerm) || u.Profile.FirstName.Contains(searchTerm))) 
            .OrderBy(u => u.Email)
            .Take(request.Limit) 
            .Select(u => new UserAutocompleteDto(
                u.Id,
                u.Email,
                u.Profile.FirstName,
                u.Profile.AvatarUrl
            ))
            .ToListAsync(cancellationToken);

        return users;
    }
}
