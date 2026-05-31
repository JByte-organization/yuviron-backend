using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions; // Твои ApplySorting и ToPaginatedListAsync

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (request.Types != null && request.Types.Any())
        {
            query = query.Where(n => n.EntityType.HasValue && request.Types.Contains(n.EntityType.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(n => n.Title.Contains(request.SearchTerm) || n.Body.Contains(request.SearchTerm));
        }

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Domain.Entities.Notification.CreatedAt),
            defaultDesc: true
        );

        var projectedQuery = sortedQuery.Select(n => new NotificationDto(
            n.Id,
            n.Title,
            n.Body,
            n.EntityType.HasValue ? n.EntityType.Value.ToString() : null,
            n.EntityId,
            n.IsRead,
            n.CreatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}