using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBanners;

public sealed class GetBannersHandler : IRequestHandler<GetBannersQuery, PaginatedList<BannerListItemDto>>
{
    private readonly IContentContext _contentContext;

    public GetBannersHandler(IContentContext contentContext)
    {
        _contentContext = contentContext;
    }

    public async Task<PaginatedList<BannerListItemDto>> Handle(GetBannersQuery request, CancellationToken cancellationToken)
    {
        return await _contentContext.Banners
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BannerListItemDto(
                b.Id,
                b.Title,
                b.BannerUrl,
                b.IsActive,
                b.StartsAtUtc,
                b.EndsAtUtc
            ))
            .ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
