using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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

        var defaultRole = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == "User", cancellationToken)
            ?? throw new InvalidOperationException("Default role 'User' is not configured.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime; 

        var user = User.Create(
            normalizedEmail, passwordHash, request.AcceptMarketing, request.AcceptTerms, utcNow);

        var profile = UserProfile.Create(
            user.Id, firstName, null, null, null, request.DateOfBirth, request.Gender, utcNow);

        user.SetProfile(profile);
        
        var userRole = UserRole.Create(user.Id, defaultRole.Id);
        user.UserRoles.Add(userRole);

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailViolation(ex))
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        var permissions = await _permissionService.CachePermissionsAsync(user, cancellationToken);
        var token = _jwtTokenGenerator.GenerateToken(user);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(
            user.Id, hashedRefreshToken, utcNow.AddDays(30), utcNow);

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

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