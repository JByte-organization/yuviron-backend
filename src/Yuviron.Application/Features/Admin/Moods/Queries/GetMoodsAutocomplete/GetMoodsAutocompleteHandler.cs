using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Moods.Queries.GetMoodsAutocomplete;

public sealed class GetMoodsAutocompleteHandler : IRequestHandler<GetMoodsAutocompleteQuery, List<MoodAutocompleteDto>>
{
    private readonly ICatalogContext _catalogContext;

    public GetMoodsAutocompleteHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<List<MoodAutocompleteDto>> Handle(GetMoodsAutocompleteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return new List<MoodAutocompleteDto>();

        var searchTerm = request.SearchTerm.Trim();

        return await _catalogContext.Moods
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.Name.Contains(searchTerm))
            .OrderBy(m => m.Name)
            .Take(request.Limit)
            .Select(m => new MoodAutocompleteDto(
                m.Id,
                m.Name,
                m.CoverUrl
            ))
            .ToListAsync(cancellationToken);
    }
}