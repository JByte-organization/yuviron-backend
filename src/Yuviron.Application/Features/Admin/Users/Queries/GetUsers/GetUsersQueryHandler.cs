using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs; // Для PaginatedList
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
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

        // Фильтры
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // MySQL case-insensitive поиск по Email или Имени в профиле
            query = query.Where(u => 
                u.Email.Contains(request.SearchTerm) || 
                (u.Profile != null && u.Profile.DisplayName.Contains(request.SearchTerm)));
        }

        if (request.AccountState.HasValue)
        {
            query = query.Where(u => u.AccountState == request.AccountState.Value);
        }

        // Подсчет общего количества
        var totalCount = await query.CountAsync(cancellationToken);

        // Пагинация и маппинг. EF Core сам сделает LEFT JOIN к таблице UserProfiles!
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email,
                u.Profile != null ? u.Profile.DisplayName : null,
                u.AccountState,
                u.IsDeleted,
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<UserListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}