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

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequestById;

public sealed class GetBannerRequestByIdHandler : IRequestHandler<GetBannerRequestByIdQuery, BannerRequestDetailsDto>
{
    private readonly IContentContext _contentContext;

    public GetBannerRequestByIdHandler(IContentContext contentContext)
    {
        _contentContext = contentContext;
    }

    public async Task<BannerRequestDetailsDto> Handle(GetBannerRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var bannerReq = await _contentContext.BannerRequests
            .AsNoTracking()
            .Include(br => br.Artist)
            .Include(br => br.Album)
            .Where(br => br.Id == request.RequestId)
            .Select(br => new BannerRequestDetailsDto(
                br.Id,
                br.ArtistId,
                br.Artist.Name,
                br.AlbumId,
                br.Album != null ? br.Album.Title : null,
                br.Title,
                br.BannerUrl,
                br.Status,
                br.AdminNotes,
                br.IsPaid,
                br.DurationDays,
                br.TargetCountries,
                br.TargetGenres,
                br.EndsAtUtc,
                br.CreatedAt,
                br.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(BannerRequest), request.RequestId);

        return bannerReq;
    }
}
