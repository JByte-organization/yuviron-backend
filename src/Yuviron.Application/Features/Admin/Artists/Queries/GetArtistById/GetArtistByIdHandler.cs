using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdHandler : IRequestHandler<GetArtistByIdQuery, ArtistDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetArtistByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ArtistDetailsDto> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
            .AsNoTracking()
            .Where(a => a.Id == request.ArtistId && !a.IsDeleted)
            .Select(a => new ArtistDetailsDto(
                a.Id,
                a.TeamMembers
                    .Where(tm => tm.Role == ArtistTeamRole.Owner && !tm.User.IsDeleted)
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
                a.AlbumArtists.Count(aa => !aa.Album.IsDeleted), 
                a.TrackArtists.Count(ta => !ta.Track.IsDeleted), 
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (artist is null) throw new NotFoundException(nameof(Artist), request.ArtistId);

        return artist;
    }
}
