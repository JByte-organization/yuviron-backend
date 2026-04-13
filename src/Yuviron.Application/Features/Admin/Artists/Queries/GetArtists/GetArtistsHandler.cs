using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Enums;

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

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Name.StartsWith(request.SearchTerm));

        if (request.VerificationStatus.HasValue)
            query = query.Where(a => a.VerificationStatus == request.VerificationStatus.Value);

        var projectedQuery = query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ArtistListItemDto(
                a.Id,
                a.Name,
                a.AvatarUrl,
                a.TeamMembers
                    .Where(tm => tm.Role == ArtistTeamRole.Owner)
                    .Select(tm => tm.User.Email)
                    .FirstOrDefault(),
                a.VerificationStatus,
                a.AlbumArtists.Count, 
                a.CreatedAt,
                a.UpdatedAt
            ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}