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
        var artistExists = await _context.Artists
            .AsNoTracking()
            .AnyAsync(a => a.Id == request.ArtistId && !a.IsDeleted, cancellationToken);

        if (!artistExists)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

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
                a.MonthlyListenersCount
            ))
            .FirstAsync(cancellationToken);

        return artist;
    }
}
