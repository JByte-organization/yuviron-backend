using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed class CreateArtistHandler : IRequestHandler<CreateArtistCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateArtistHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateArtistCommand request, CancellationToken cancellationToken)
    {
        User? ownerUser = null;

        if (request.OwnerUserId.HasValue)
        {
            ownerUser = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == request.OwnerUserId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.OwnerUserId.Value);
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

        if (ownerUser != null)
        {
            var managementRoleStr = nameof(RoleName.ManagementUser);
            var managementRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == managementRoleStr, cancellationToken)
                ?? throw new InvalidOperationException($"Role '{managementRoleStr}' not found.");

            var currentRoleIds = ownerUser.UserRoles.Select(ur => ur.RoleId).ToList();
            
            if (!currentRoleIds.Contains(managementRole.Id))
            {
                currentRoleIds.Add(managementRole.Id);
                
                ownerUser.SyncRoles(currentRoleIds);
                
                ownerUser.AddDomainEvent(new UserPermissionsChangedEvent(ownerUser.Id));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return artist.Id;
    }
}