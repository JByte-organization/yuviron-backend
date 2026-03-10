using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterHandler> _logger;
    private readonly TimeProvider _timeProvider; 
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPermissionService _permissionService;
    

    public RegisterHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ILogger<RegisterHandler> logger,
        TimeProvider timeProvider, 
        IJwtTokenGenerator jwtTokenGenerator,
        IPermissionService permissionService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
        _timeProvider = timeProvider; 
        _jwtTokenGenerator = jwtTokenGenerator;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);
        var firstName = request.FirstName.Trim();

        var emailExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists) throw new UserAlreadyExistsException(normalizedEmail);

        var roleNamesToAssign = new List<string> { "User" };
        if (request.IsArtist)
        {
            roleNamesToAssign.Add("ManagementUser");
        }

        var rolesToAssign = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => roleNamesToAssign.Contains(r.Name))
            .ToListAsync(cancellationToken);

        if (!rolesToAssign.Any(r => r.Name == "User"))
            throw new InvalidOperationException("Default role 'User' is not configured.");
        if (request.IsArtist && !rolesToAssign.Any(r => r.Name == "ManagementUser"))
            throw new InvalidOperationException("Role 'ManagementUser' is not configured in the database.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime; 

        var user = User.Create(
            normalizedEmail, passwordHash, request.AcceptMarketing, request.AcceptTerms, utcNow);

        var profile = UserProfile.Create(
            user.Id, firstName, null, null, null, request.DateOfBirth, request.Gender, utcNow);

        user.SetProfile(profile);
        
        foreach (var role in rolesToAssign)
        {
            user.UserRoles.Add(new UserRole(user.Id, role.Id));
        }

        _context.Users.Add(user);

        if (request.IsArtist && !string.IsNullOrWhiteSpace(request.ArtistName))
        {
            var artist = Artist.Create(
                initialOwnerUserId: user.Id, 
                name: request.ArtistName, 
                bio: null, 
                avatarUrl: null, 
                bannerUrl: null, 
                verificationStatus: VerificationStatus.None, 
                utcNow: utcNow);

            _context.Artists.Add(artist);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(
            user.Id, hashedRefreshToken, utcNow.AddDays(30), utcNow);

        _context.RefreshTokens.Add(refreshTokenEntity);

        var permissions = _permissionService.CalculateUserPermissions(user, utcNow);

        user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id));

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailViolation(ex))
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        try
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Добро пожаловать в Yuviron!",
                $"<h1>Привет, {firstName}!</h1><p>Спасибо за регистрацию.</p>",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        return new LoginResponse(
            user.Id,
            token,
            rawRefreshToken, 
            user.Email,
            permissions
        );
    }

    private static bool IsDuplicateEmailViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
               && message.Contains("users", StringComparison.OrdinalIgnoreCase)
               && message.Contains("email", StringComparison.OrdinalIgnoreCase);
    }
}