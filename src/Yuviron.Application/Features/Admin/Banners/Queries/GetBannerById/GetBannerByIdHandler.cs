using MediatR;
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
    private readonly IApplicationDbContext _context;

    public GetBannerByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BannerDetailsDto> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners
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
