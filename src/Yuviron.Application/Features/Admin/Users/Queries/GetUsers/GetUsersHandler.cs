using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider; 

    public GetUsersHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => 
                u.Email.Contains(request.SearchTerm) || 
                u.Profile.FirstName.Contains(request.SearchTerm));
        }

        if (request.AccountState.HasValue)
        {
            query = query.Where(u => u.AccountState == request.AccountState.Value);
        }
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var projectedQuery = query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email,
                u.Profile.FirstName, 
                u.Profile.AvatarUrl, 
                u.AccountState,
                u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow),
                u.CreatedAt,
                u.UpdatedAt,
                u.LastLoginAt,
                u.UserRoles.Select(ur => ur.Role.Name).ToList()
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}