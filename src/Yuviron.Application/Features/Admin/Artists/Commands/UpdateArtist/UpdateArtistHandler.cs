using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; // <-- Для файлов
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistHandler : IRequestHandler<UpdateArtistCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- Инжектим сервис файлов

    public UpdateArtistHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdateArtistCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers) 
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        // 1. Проверяем, существует ли вообще новый владелец (без всяких .HasValue)
        var ownerExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.OwnerUserId, cancellationToken);

        if (!ownerExists) throw new NotFoundException(nameof(User), request.OwnerUserId);

        // 2. Запоминаем старые картинки до обновления (для удаления)
        var oldAvatarUrl = artist.AvatarUrl;
        var oldBannerUrl = artist.BannerUrl;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // 3. Обновляем базовые данные
        artist.UpdateDetails(
            request.Name,
            request.Bio,
            request.AvatarUrl,
            request.BannerUrl,
            request.VerificationStatus,
            utcNow);

        // 4. Умная логика передачи прав (Магия для админа)
        var currentOwner = artist.TeamMembers.FirstOrDefault(tm => tm.Role == ArtistTeamRole.Owner);
        var newOwnerId = request.OwnerUserId;

        if (currentOwner?.UserId != newOwnerId)
        {
            var isNewOwnerInTeam = artist.TeamMembers.Any(tm => tm.UserId == newOwnerId);

            if (isNewOwnerInTeam)
            {
                // Если юзер уже в команде — просто даем ему корону
                artist.UpdateTeamMemberRole(newOwnerId, ArtistTeamRole.Owner, utcNow);
            }
            else
            {
                // Если юзера нет в команде: 
                // Понижаем текущего владельца (если он есть) до менеджера
                if (currentOwner != null)
                {
                    artist.UpdateTeamMemberRole(currentOwner.UserId, ArtistTeamRole.Manager, utcNow);
                }
                
                // Добавляем нового человека сразу как владельца
                artist.AddTeamMember(newOwnerId, ArtistTeamRole.Owner, utcNow);
            }

            var newOwnerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == newOwnerId, cancellationToken);
            if (newOwnerUser != null) newOwnerUser.AddDomainEvent(new UserPermissionsChangedEvent(newOwnerId));

            if (currentOwner != null)
            {
                var oldOwnerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentOwner.UserId, cancellationToken);
                if (oldOwnerUser != null) oldOwnerUser.AddDomainEvent(new UserPermissionsChangedEvent(oldOwnerUser.Id));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.Equals(oldAvatarUrl, request.AvatarUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            await _fileStorageService.DeleteAsync(oldAvatarUrl, cancellationToken);
        }

        if (!string.Equals(oldBannerUrl, request.BannerUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldBannerUrl))
        {
            await _fileStorageService.DeleteAsync(oldBannerUrl, cancellationToken);
        }

        return Unit.Value;
    }
}