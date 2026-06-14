using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Auth.Commands.AdminPreLogin;

public record AdminLoginCodeModel(string Code);

public sealed class AdminPreLoginHandler : IRequestHandler<AdminPreLoginCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminPreLoginHandler> _logger;
    private readonly ITemplateService _templateService; 

    public AdminPreLoginHandler(
        IIdentityContext identityContext,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<AdminPreLoginHandler> logger,
        ITemplateService templateService) 
    {
        _identityContext = identityContext;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
        _templateService = templateService;
    }

    public async Task<Unit> Handle(AdminPreLoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var user = await _identityContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        bool isAdmin = user.UserRoles.Any(ur => 
            ur.Role.RolePermissions.Any(rp => rp.Permission.Name == nameof(AppPermission.AccessAdminPanel)));

        if (!isAdmin)
        {
            _logger.LogWarning("Security: User {UserId} attempted to log into Admin Panel without permissions.", user.Id);
            throw new UnauthorizedAccessException("Access denied. Admin privileges required.");
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var codeHash = _passwordHasher.Hash(code);

        await _otpService.SaveAdminLoginCodeHashAsync(
            normalizedEmail, 
            codeHash, 
            TimeSpan.FromMinutes(10), 
            cancellationToken);

        try
        {
            var htmlBody = await _templateService.RenderTemplateAsync("LoginCode", new AdminLoginCodeModel(code));

            await _emailService.SendEmailAsync(
                normalizedEmail,
                "Yuviron Admin Panel - Login Code",
                htmlBody, 
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send admin login code to {Email}", normalizedEmail);
        }

        return Unit.Value;
    }
}