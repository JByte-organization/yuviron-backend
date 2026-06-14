using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdHandler : IRequestHandler<GetArtistByIdQuery, ArtistDetailsDto>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetArtistByIdHandler(
        ICatalogContext catalogContext,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<ArtistDetailsDto> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken)
    {
        var artist = await _catalogContext.Artists
            .AsNoTracking()
            .Where(a => a.Id == request.ArtistId)
            .Select(a => new ArtistDetailsDto(
                a.Id,
                a.Name,
                a.Bio,
                a.AvatarUrl,
                a.BannerUrl,
                a.VerificationStatus,
                a.MonthlyListenersCount,
                a.SocialLinks.Select(sl => new ClientSocialLinkDto(
                    sl.Type, 
                    sl.Url
                )).ToList(),
                false 
            ))
            .FirstOrDefaultAsync(cancellationToken); 

        if (artist == null)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        return await artist.EnrichWithCacheAsync(
            _cache, 
            _currentUser.UserId, 
            "followed_artists", 
            x => x.Id, 
            (x, followed) => x with { IsFollowed = followed }, 
            cancellationToken);
    }
}
