using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;


namespace Yuviron.Application.Features.Admin.Ads.Queries.GetAdById;

public sealed class GetAdByIdHandler : IRequestHandler<GetAdByIdQuery, AdDetailsDto>
{
    private readonly IMonetizationContext _monetizationContext;

    public GetAdByIdHandler(IMonetizationContext monetizationContext)
    {
        _monetizationContext = monetizationContext;
    }

    public async Task<AdDetailsDto> Handle(GetAdByIdQuery request, CancellationToken cancellationToken)
    {
        var ad = await _monetizationContext.Ads
            .AsNoTracking()
            .Where(a => a.Id == request.AdId)
            .Select(a => new AdDetailsDto(
                a.Id,
                a.AdvertiserName,
                a.Title,
                a.AudioUrl,
                a.ImageUrl,
                a.ClickUrl,
                a.IsActive,
                a.Impressions.Count,
                a.Impressions.Count(i => i.IsClicked),
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (ad == null)
        {
            throw new NotFoundException(nameof(Ad), request.AdId);
        }

        return ad;
    }
}