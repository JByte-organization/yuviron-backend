using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsersAutocomplete;

public sealed class GetUsersAutocompleteHandler : IRequestHandler<GetUsersAutocompleteQuery, List<UserAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersAutocompleteHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserAutocompleteDto>> Handle(GetUsersAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return new List<UserAutocompleteDto>();
        }

        var searchTerm = request.SearchTerm.Trim().ToLower();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Email.ToLower().Contains(searchTerm) || 
                        (u.Profile != null && u.Profile.DisplayName.ToLower().Contains(searchTerm)))
            .OrderBy(u => u.Email)
            .Take(request.Limit) 
            .Select(u => new UserAutocompleteDto(
                u.Id,
                u.Email,
                u.Profile != null ? u.Profile.DisplayName : "No Name",
                u.Profile != null ? u.Profile.AvatarUrl : null
            ))
            .ToListAsync(cancellationToken);

        return users;
    }
}