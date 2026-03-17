using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtists;

public sealed class GetArtistsHandler : IRequestHandler<GetArtistsQuery, PaginatedList<ArtistListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtistsHandler(IApplicationDbContext context)
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

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.Name.StartsWith(request.SearchTerm));
        }

        if (request.VerificationStatus.HasValue)
        {
            query = query.Where(a => a.VerificationStatus == request.VerificationStatus.Value);
        }

        var projectedQuery = query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ArtistListItemDto(
                a.Id,
                a.Name,
                a.VerificationStatus,
                a.CreatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}