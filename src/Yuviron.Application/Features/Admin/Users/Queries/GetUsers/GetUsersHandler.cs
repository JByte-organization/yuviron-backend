using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if (request.IncludeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => 
                u.Email.StartsWith(request.SearchTerm) || 
                (u.Profile != null && u.Profile.DisplayName.StartsWith(request.SearchTerm)));
        }

        if (request.AccountState.HasValue)
        {
            query = query.Where(u => u.AccountState == request.AccountState.Value);
        }
        
        var projectedQuery = query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email,
                u.Profile != null ? u.Profile.DisplayName : null,
                u.AccountState,
                u.IsDeleted,
                u.CreatedAt,
                u.UserRoles.Select(ur => ur.Role.Name).ToList()
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}