using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Banners.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Queries.GetBannerById;

public sealed class GetBannerByIdHandler : IRequestHandler<GetBannerByIdQuery, BannerDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetBannerByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<BannerDetailsDto> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners
            .AsNoTracking()
            .Where(b => b.Id == request.BannerId)
            .Select(b => new BannerDetailsDto(
                b.Id,
                b.Title,
                b.BannerUrl,
                b.TargetUrl,
                b.SortOrder,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (banner is null)
            throw new NotFoundException(nameof(Banner), request.BannerId);

        return banner;
    }
}