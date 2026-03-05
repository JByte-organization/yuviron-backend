using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs; // Путь к твоему PaginatedList
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    string? SearchTerm, // Поиск по Email или Имени
    AccountState? AccountState, // Фильтр по статусу (Active, Banned, etc)
    bool IncludeDeleted, // Показывать ли удаленных
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedList<UserListItemDto>>, ISecuredRequest
{
    // Обрати внимание: права на юзеров обычно отделены от прав на каталог!
    public AppPermission RequiredPermission => AppPermission.ManageUsers; 
}