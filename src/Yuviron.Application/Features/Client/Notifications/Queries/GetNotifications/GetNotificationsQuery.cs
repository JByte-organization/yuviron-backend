using MediatR;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    List<NotificationEntityType>? Types = null,
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<NotificationDto>>;