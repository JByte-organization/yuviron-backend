using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterHandler> _logger;
    private readonly TimeProvider _timeProvider; 

    public RegisterHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<RegisterHandler> logger,
        TimeProvider timeProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _timeProvider = timeProvider; 
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);
        var firstName = request.FirstName.Trim();

        var emailExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists) throw new UserAlreadyExistsException(normalizedEmail);

        var roleNamesToAssign = new List<string> { nameof(RoleName.User) };
        if (request.IsArtist)
        {
            roleNamesToAssign.Add(nameof(RoleName.ManagementUser));
        }

        var rolesToAssign = await _context.Roles
            .AsNoTracking()
            .Where(r => roleNamesToAssign.Contains(r.Name))
            .ToListAsync(cancellationToken);

        if (!rolesToAssign.Any(r => r.Name == nameof(RoleName.User)))
            throw new InvalidOperationException($"Default role '{nameof(RoleName.User)}' is not configured.");
        
        if (request.IsArtist && !rolesToAssign.Any(r => r.Name == nameof(RoleName.ManagementUser)))
            throw new InvalidOperationException($"Role '{nameof(RoleName.ManagementUser)}' is not configured in the database.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime; 

        var user = User.Create(
            normalizedEmail, passwordHash, firstName, request.AcceptMarketing, request.AcceptTerms, utcNow);

        var profile = UserProfile.Create(
            userId: user.Id, 
            firstName: firstName, 
            avatarUrl: null, 
            bannerUrl: null,
            country: request.Country, 
            city: request.City,       
            bio: null, 
            dateOfBirth: request.DateOfBirth, 
            gender: request.Gender, 
            utcNow: utcNow);

        user.SetProfile(profile);
        
        foreach (var role in rolesToAssign)
        {
            user.UserRoles.Add(new UserRole(user.Id, role.Id));
        }

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