using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; // <-- ДОБАВИТЬ
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.DeleteArtist;

public sealed class DeleteArtistHandler : IRequestHandler<DeleteArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- ДОБАВИТЬ

    public DeleteArtistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) // <-- ДОБАВИТЬ
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(DeleteArtistCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        // <-- Запоминаем пути к файлам ДО удаления
        var avatarUrlToDelete = artist.AvatarUrl;
        var bannerUrlToDelete = artist.BannerUrl;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        artist.Delete(utcNow);

        var memberUserIds = artist.TeamMembers.Select(tm => tm.UserId).ToList();

        var usersWithOtherArtists = await _context.ArtistTeamMembers
            .Where(tm => memberUserIds.Contains(tm.UserId) && tm.ArtistId != request.ArtistId)
            .Select(tm => tm.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var userIdsToRevokeRole = memberUserIds.Except(usersWithOtherArtists).ToList();

        if (userIdsToRevokeRole.Any())
        {
            var managementRoleStr = nameof(RoleName.ManagementUser);
            var managementRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == managementRoleStr, cancellationToken);

            if (managementRole != null)
            {
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .Where(u => userIdsToRevokeRole.Contains(u.Id))
                    .ToListAsync(cancellationToken);

                foreach (var user in users)
                {
                    user.SyncRoles(user.UserRoles
                        .Where(ur => ur.RoleId != managementRole.Id)
                        .Select(ur => ur.RoleId));
                
                    user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        if (!string.IsNullOrWhiteSpace(avatarUrlToDelete))
        {
            await _fileStorageService.DeleteAsync(avatarUrlToDelete, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(bannerUrlToDelete))
        {
            await _fileStorageService.DeleteAsync(bannerUrlToDelete, cancellationToken);
        }

        return Unit.Value;
    }
}