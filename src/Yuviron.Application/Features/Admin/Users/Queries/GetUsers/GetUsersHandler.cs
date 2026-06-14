using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserListItemDto>>
{
    private readonly IIdentityContext _identityContext;
    private readonly TimeProvider _timeProvider;

    public GetUsersHandler(IIdentityContext identityContext, TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _identityContext.Users
            .AsNoTracking()
            .WhereHasPermission(request.RequiredPermission);

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

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(User.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<User, object>>>
            {
                ["FirstName"] = u => u.Profile.FirstName,

                ["IsPremium"] = u => u.Subscriptions.Any(s => 
                    s.Status == SubscriptionStatus.Active && s.EndAt > utcNow),

                ["Roles"] = u => u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault()!
            });

        var projectedQuery = sortedQuery.Select(u => new UserListItemDto(
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
