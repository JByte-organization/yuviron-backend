using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services; // <-- Добавили для файлов
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public UpdateUserHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var emailTaken = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != request.UserId && u.Email == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        user.UpdateAdminDetails(
            normalizedEmail,
            request.AcceptMarketing,
            request.AccountState,
            utcNow);


        var oldAvatarUrl = user.Profile?.AvatarUrl;

       
        var finalAvatarUrl = await _fileStorageService.MoveIfTempAsync(request.AvatarUrl, "avatars", cancellationToken);

        if (user.Profile == null)
        {
            user.SetProfile(UserProfile.Create(
                user.Id,
                request.DisplayName.Trim(),
                finalAvatarUrl, 
                null,
                null,
                request.DateOfBirth,
                request.Gender,
                utcNow));
        }
        else
        {
            user.Profile.UpdateDetails(
                request.DisplayName.Trim(),
                finalAvatarUrl, 
                user.Profile.Country,
                user.Profile.Bio,
                request.DateOfBirth,
                request.Gender,
                utcNow);
        }

        if (request.RoleIds is not null)
        {
            var requestedRoleIds = request.RoleIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToHashSet();

            var existingRoleIds = await _context.Roles
                .AsNoTracking()
                .Where(r => requestedRoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (existingRoleIds.Count != requestedRoleIds.Count)
            {
                throw new NotFoundException(nameof(Role), "One or more role IDs");
            }

            user.SyncRoles(existingRoleIds);
        }

        try
        {
            user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id)); 
            
            await _context.SaveChangesAsync(cancellationToken);

            if (!string.Equals(oldAvatarUrl, finalAvatarUrl, StringComparison.OrdinalIgnoreCase) 
                && !string.IsNullOrWhiteSpace(oldAvatarUrl))
            {
                await _fileStorageService.DeleteAsync(oldAvatarUrl, cancellationToken);
            }
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailViolation(ex))
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        return Unit.Value;
    }

    private static bool IsDuplicateEmailViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
               && message.Contains("users", StringComparison.OrdinalIgnoreCase)
               && message.Contains("email", StringComparison.OrdinalIgnoreCase);
    }
}