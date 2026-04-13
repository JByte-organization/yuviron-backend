using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;
using Yuviron.Application.Extensions; 

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateUserHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
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

        var oldAvatarUrl = user.Profile!.AvatarUrl;
        var finalAvatarUrl = FileStorageExtensions.PredictDestinationPath(request.AvatarUrl, "avatars");

        user.Profile!.UpdateDetails(
            request.FirstName.Trim(),
            finalAvatarUrl, 
            user.Profile.Country,
            user.Profile.Bio,
            request.DateOfBirth,
            request.Gender,
            utcNow);

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

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl) && request.AvatarUrl.StartsWith("temp/"))
        {
            user.AddDomainEvent(new TempFileNeedsMovingEvent(request.AvatarUrl, "avatars"));
        }

        if (!string.Equals(oldAvatarUrl, finalAvatarUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            user.AddDomainEvent(new FileNeedsDeletionEvent(oldAvatarUrl));
        }

        try
        {
            user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id)); 
            
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