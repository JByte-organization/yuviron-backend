using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerById;

public sealed class GetBannerByIdHandler : IRequestHandler<GetBannerByIdQuery, BannerDetailsDto>
{
    private readonly IContentContext _contentContext;

    public GetBannerByIdHandler(IContentContext contentContext)
    {
        _contentContext = contentContext;
    }

    public async Task<BannerDetailsDto> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        var banner = await _contentContext.Banners
            .AsNoTracking()
            .Include(b => b.Artist)
            .Where(b => b.Id == request.BannerId)
            .Select(b => new BannerDetailsDto(
                b.Id,
                b.ArtistId,
                b.Artist != null ? b.Artist.Name : null,
                b.Title,
                b.BannerUrl,
                b.TargetUrl,
                b.IsActive,
                b.StartsAtUtc,
                b.EndsAtUtc,
                b.TargetCountries,
                b.TargetGenres,
                b.CreatedAt,
                b.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Banner), request.BannerId);

        return banner;
    }
}
