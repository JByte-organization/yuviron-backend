using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication; 
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateTeamMemberRole;

public sealed class UpdateTeamMemberRoleHandler : IRequestHandler<UpdateTeamMemberRoleCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UpdateTeamMemberRoleHandler> _logger; 
    private readonly IIdentityManager _identityManager;

    public UpdateTeamMemberRoleHandler(
        ICatalogContext catalogContext, 
        TimeProvider timeProvider,
        IPermissionService permissionService,
        ILogger<UpdateTeamMemberRoleHandler> logger,
        IIdentityManager identityManager) 
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
        _logger = logger;
        _identityManager = identityManager;
    }

    public async Task<Unit> Handle(UpdateTeamMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var artist = await _catalogContext.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.UpdateTeamMemberRole(request.UserId, request.NewRole, utcNow);

        await _identityManager.EnsureManagementRoleAsync(request.UserId, cancellationToken);

        artist.AddDomainEvent(new UserPermissionsChangedEvent(request.UserId));

        await _catalogContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _permissionService.InvalidatePermissionsAsync(request.UserId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache synchronously for User {UserId}. Outbox worker will retry.", request.UserId);
        }

        return Unit.Value;
    }
}