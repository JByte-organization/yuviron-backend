using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistsAutocomplete;

public sealed class GetArtistsAutocompleteHandler : IRequestHandler<GetArtistsAutocompleteQuery, List<ArtistAutocompleteDto>>
{
    private readonly ICatalogContext _catalogContext;

    public GetArtistsAutocompleteHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<List<ArtistAutocompleteDto>> Handle(GetArtistsAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<ArtistAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim().ToLower();

        return await _catalogContext.Artists
            .AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(searchTerm))
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