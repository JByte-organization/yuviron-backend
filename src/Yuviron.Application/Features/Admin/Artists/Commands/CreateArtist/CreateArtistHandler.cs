using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed class CreateArtistCommandHandler : IRequestHandler<CreateArtistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateArtistCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateArtistCommand request, CancellationToken cancellationToken)
    {
        if (request.OwnerUserId.HasValue)
        {
            var ownerExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == request.OwnerUserId.Value, cancellationToken);

            if (!ownerExists) throw new NotFoundException(nameof(User), request.OwnerUserId.Value);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var artist = Artist.Create(
            request.OwnerUserId,
            request.Name,
            request.Bio,
            request.AvatarUrl,
            request.BannerUrl,
            request.VerificationStatus,
            utcNow);

        _context.Artists.Add(artist);
        await _context.SaveChangesAsync(cancellationToken);

        return artist.Id;
    }
}