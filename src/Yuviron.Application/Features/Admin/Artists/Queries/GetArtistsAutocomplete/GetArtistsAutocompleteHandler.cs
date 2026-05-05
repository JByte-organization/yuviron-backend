using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums; // Убедись, что тут лежит твой ArtistTeamRole

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistsAutocomplete;

public sealed class GetArtistsAutocompleteHandler : IRequestHandler<GetArtistsAutocompleteQuery, List<ArtistAutocompleteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetArtistsAutocompleteHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArtistAutocompleteDto>> Handle(GetArtistsAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<ArtistAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim().ToLower();

        return await _context.Artists
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Name.ToLower().Contains(searchTerm))
            .OrderBy(a => a.Name)
            .Take(request.Limit)
            .Select(a => new ArtistAutocompleteDto(
                a.Id,
                a.Name,
                a.AvatarUrl,
                a.TeamMembers
                    .Where(tm => tm.Role == ArtistTeamRole.Owner)
                    .Select(tm => tm.User.Email)
                    .FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);
    }
}