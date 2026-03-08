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

namespace Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;

public sealed class AddTeamMemberHandler : IRequestHandler<AddTeamMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public AddTeamMemberHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var user = await _context.Users
            .Include(u => u.UserRoles) 
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.AddTeamMember(request.UserId, request.Role, utcNow);

        var managementRoleStr = nameof(RoleName.ManagementUser);
        var managementRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == managementRoleStr, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{managementRoleStr}' not found.");

        var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        if (!currentRoleIds.Contains(managementRole.Id))
        {
            currentRoleIds.Add(managementRole.Id);
            
            user.SyncRoles(currentRoleIds);
            
            user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}