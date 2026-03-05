using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs; // Берем PaginatedList отсюда (или из Common, если ты его перенес)
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;

public sealed record GetArtistsQuery(
    string? SearchTerm, // Поиск по имени
    VerificationStatus? VerificationStatus, // Фильтр по статусу галочки
    bool IncludeDeleted, // Показывать ли удаленных
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedList<ArtistListItemDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}