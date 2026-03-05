using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public UpdateUserCommandHandler(
        IApplicationDbContext context, 
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider) // ИСПРАВЛЕНО: Добавили в инъекцию
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider; // ИСПРАВЛЕНО: Инициализировали
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
            request.AcceptTerms,
            request.AccountState,
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.SetPasswordHash(_passwordHasher.Hash(request.Password), utcNow);
        }

        if (user.Profile == null)
        {
            user.SetProfile(UserProfile.Create(
                user.Id,
                request.DisplayName.Trim(),
                null, // AvatarUrl
                null, // Country
                null, // Bio
                request.DateOfBirth,
                request.Gender,
                utcNow));
        }
        else
        {
            // ИСПРАВЛЕНО: Теперь UpdateDetails и сохраняем старые аватарку/био/страну, чтобы не затереть
            user.Profile.UpdateDetails(
                request.DisplayName.Trim(),
                user.Profile.AvatarUrl,
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
            await _context.SaveChangesAsync(cancellationToken);
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