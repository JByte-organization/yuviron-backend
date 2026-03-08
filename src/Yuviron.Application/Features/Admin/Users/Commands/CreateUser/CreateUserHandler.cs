using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;


    public CreateUserCommandHandler(
        IApplicationDbContext context, 
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var emailExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = User.Create(
            normalizedEmail,
            passwordHash,
            request.AcceptMarketing,
            request.AcceptTerms,
            utcNow); 

        user.UpdateAdminDetails(
            normalizedEmail,
            request.AcceptMarketing,
            request.AcceptTerms,
            request.AccountState,
            utcNow); 

        var profile = UserProfile.Create(
            user.Id,
            request.DisplayName.Trim(),
            null,
            null,
            null, 
            request.DateOfBirth,
            request.Gender,
            utcNow);

        user.SetProfile(profile);

        var requestedRoleIds = request.RoleIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (requestedRoleIds is { Length: > 0 })
        {
            var existingRoleIds = await _context.Roles
                .AsNoTracking()
                .Where(r => requestedRoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (existingRoleIds.Count != requestedRoleIds.Length)
            {
                throw new NotFoundException(nameof(Role), "One or more role IDs");
            }

            user.SyncRoles(existingRoleIds);
        }
        else
        {
            var userRoleStr = nameof(RoleName.User); 
            
            var defaultRoleId = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Name == userRoleStr)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (defaultRoleId == Guid.Empty)
            {
                throw new NotFoundException(nameof(Role), userRoleStr);
            }

            user.SyncRoles(new[] { defaultRoleId });
        }

        user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailViolation(ex))
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }


        return user.Id;
    }

    private static bool IsDuplicateEmailViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
               && message.Contains("users", StringComparison.OrdinalIgnoreCase)
               && message.Contains("email", StringComparison.OrdinalIgnoreCase);
    }
}