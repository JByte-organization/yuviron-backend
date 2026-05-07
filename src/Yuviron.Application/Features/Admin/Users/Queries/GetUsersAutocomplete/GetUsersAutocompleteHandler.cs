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

        var searchTerm = request.SearchTerm.Trim();

        var users = await _context.Users
            .AsNoTracking()
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