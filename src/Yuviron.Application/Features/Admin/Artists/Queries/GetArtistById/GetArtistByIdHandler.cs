using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdHandler : IRequestHandler<GetArtistByIdQuery, ArtistDetailsDto>
{
    private readonly ICatalogContext _catalogContext;

    public GetArtistByIdHandler(ICatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    public async Task<ArtistDetailsDto> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken)
    {
        var artist = await _catalogContext.Artists
            .AsNoTracking()
            .Where(a => a.Id == request.ArtistId)
            .Select(a => new ArtistDetailsDto(
                a.Id,
                a.TeamMembers
                    .Where(tm => tm.Role == ArtistTeamRole.Owner)
                    .Select(tm => new ArtistOwnerDto(
                        tm.UserId,
                        tm.User.Email,
                        tm.User.Profile.FirstName
                    ))
                    .FirstOrDefault(), 
                a.Name,
                a.Bio,
                a.AvatarUrl,
                a.BannerUrl,
                a.VerificationStatus,
                a.AlbumArtists.Count(), 
                a.TrackArtists.Count(), 
                
                a.SocialLinks.Select(sl => new AdminSocialLinkDto(sl.Type, sl.Url)).ToList(),
                a.Pins.OrderBy(p => p.Position).Select(p => new AdminArtistPinDto(p.EntityType, p.EntityId, p.Position)).ToList(),
                
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (artist is null) throw new NotFoundException(nameof(Artist), request.ArtistId);

        return artist;
    }
}