using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs; // Для PaginatedList
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;

public sealed class GetArtistsQueryHandler : IRequestHandler<GetArtistsQuery, PaginatedList<ArtistListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtistsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ArtistListItemDto>> Handle(GetArtistsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Artists.AsNoTracking();

        if (request.IncludeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        // Фильтры
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // MySQL case-insensitive поиск
            query = query.Where(a => a.Name.Contains(request.SearchTerm));
        }

        if (request.VerificationStatus.HasValue)
        {
            query = query.Where(a => a.VerificationStatus == request.VerificationStatus.Value);
        }

        // Подсчет общего количества для фронтенда
        var totalCount = await query.CountAsync(cancellationToken);

        // Пагинация и маппинг в DTO прямо в базе данных
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArtistListItemDto(
                a.Id,
                a.Name,
                a.AvatarUrl,
                a.IsVerified,
                a.VerificationStatus,
                a.IsDeleted,
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<ArtistListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}