using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Admin.Themes.Queries.DTOs;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Themes.Queries.GetThemes;

public sealed class GetThemesHandler : IRequestHandler<GetThemesQuery, PaginatedList<ThemeListItemDto>>
{
    private readonly IProfileContext _profileContext;

    public GetThemesHandler(IProfileContext profileContext)
    {
        _profileContext = profileContext;
    }

    public async Task<PaginatedList<ThemeListItemDto>> Handle(GetThemesQuery request, CancellationToken cancellationToken)
    {
        var query = _profileContext.Themes
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{request.SearchTerm}%"));
        }

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Theme.Id),
            defaultDesc: true);

        return await sortedQuery
            .Select(x => new ThemeListItemDto(
                x.Id,
                x.Name,
                x.PrimaryColor,
                x.SecondaryColor,
                x.BackgroundColor,
                x.IsSystem,
                x.IsPremiumOnly,
                x.UserId))
            .ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
