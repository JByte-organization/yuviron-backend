using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Artists.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Queries.GetArtistById;

public sealed class GetArtistByIdQueryHandler : IRequestHandler<GetArtistByIdQuery, ArtistDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetArtistByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ArtistDetailsDto> Handle(GetArtistByIdQuery request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
            .AsNoTracking()
            .IgnoreQueryFilters() // Админ должен видеть даже удаленного артиста, чтобы понять, что с ним
            .Where(a => a.Id == request.ArtistId)
            .Select(a => new ArtistDetailsDto(
                a.Id,
                a.OwnerUserId,
                a.Name,
                a.Bio,
                a.AvatarUrl,
                a.BannerUrl,
                a.IsVerified,
                a.VerificationStatus,
                a.IsDeleted,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (artist is null)
        {
            throw new NotFoundException(nameof(Artist), request.ArtistId);
        }

        return artist;
    }
}