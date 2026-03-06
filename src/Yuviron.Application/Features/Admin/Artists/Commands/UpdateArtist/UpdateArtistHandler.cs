using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistHandler : IRequestHandler<UpdateArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateArtistHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers) 
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        if (request.OwnerUserId.HasValue)
        {
            var ownerExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == request.OwnerUserId.Value, cancellationToken);

            if (!ownerExists) throw new NotFoundException(nameof(User), request.OwnerUserId.Value);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.UpdateDetails(
            request.Name,
            request.Bio,
            request.AvatarUrl,
            request.BannerUrl,
            request.VerificationStatus,
            utcNow);

        var currentOwner = artist.TeamMembers.FirstOrDefault(tm => tm.Role == ArtistTeamRole.Owner);

        if (request.OwnerUserId.HasValue)
        {
            if (currentOwner?.UserId != request.OwnerUserId.Value)
            {
                if (currentOwner != null)
                {
                    artist.RemoveTeamMember(currentOwner.UserId, utcNow);
                }
                artist.UpdateTeamMemberRole(request.OwnerUserId.Value, ArtistTeamRole.Owner, utcNow);
            }
        }
        else
        {
            if (currentOwner != null)
            {
                artist.RemoveTeamMember(currentOwner.UserId, utcNow);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}