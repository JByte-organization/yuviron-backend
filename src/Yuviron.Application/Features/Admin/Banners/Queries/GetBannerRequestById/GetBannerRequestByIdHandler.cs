using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerRequestById;

public sealed class GetBannerRequestByIdHandler : IRequestHandler<GetBannerRequestByIdQuery, BannerRequestDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetBannerRequestByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BannerRequestDetailsDto> Handle(GetBannerRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var bannerReq = await _context.BannerRequests
            .AsNoTracking()
            .Include(br => br.Artist)
            .Include(br => br.Album)
            .Include(br => br.SubmittedByUser).ThenInclude(u => u.Profile)
            .Where(br => br.Id == request.RequestId)
            .Select(br => new BannerRequestDetailsDto(
                br.Id,
                br.ArtistId,
                br.Artist.Name,
                br.SubmittedByUserId,
                br.SubmittedByUser.Profile.FirstName,
                br.AlbumId,
                br.Album != null ? br.Album.Title : null,
                br.Title,
                br.BannerUrl,
                br.Status,
                br.IsPaid,
                br.StripePaymentIntentId,
                br.AdminNotes,
                br.CreatedAt,
                br.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (bannerReq == null)
            throw new NotFoundException(nameof(BannerRequest), request.RequestId);

        return bannerReq;
    }
}
