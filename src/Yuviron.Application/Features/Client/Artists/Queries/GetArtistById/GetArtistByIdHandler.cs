using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;

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
                a.Name,
                a.Bio,
                a.AvatarUrl,
                a.BannerUrl,
                a.VerificationStatus,
                _context.ListeningEvents
                    .Where(le => le.UserId.HasValue &&
                                 le.MsPlayed >= 30000 &&
                                 le.Track.TrackArtists.Any(ta => ta.ArtistId == a.Id))
                    .Select(le => le.UserId!.Value)
                    .Distinct()
                    .Count()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (artist is null)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        return artist;
    }
}
