using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Auth.Commands.AdminLogin;

public sealed class AdminLoginHandler : IRequestHandler<AdminLoginCommand, LoginResponse>
{
    private readonly IIdentityContext _identityContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IOtpService _otpService;
    private readonly IPermissionService _permissionService;

    public AdminLoginHandler(
        IIdentityContext identityContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        IOtpService otpService,
        IPermissionService permissionService)
    {
        _identityContext = identityContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
        _otpService = otpService;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var cachedCodeHash = await _otpService.GetAdminLoginCodeHashAsync(normalizedEmail, cancellationToken);
        
        if (string.IsNullOrEmpty(cachedCodeHash))
        {
            throw new UnauthorizedAccessException("Code expired or not found.");
        }

        if (!_passwordHasher.Verify(request.Code, cachedCodeHash))
        {
            throw new UnauthorizedAccessException("Invalid code.");
        }

        await _otpService.RemoveAdminLoginCodeAsync(normalizedEmail, cancellationToken);

        var user = await _identityContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null || user.AccountState == AccountState.Deleted || user.IsDeleted)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        bool isAdmin = user.UserRoles.Any(ur => 
            ur.Role.RolePermissions.Any(rp => rp.Permission.Name == nameof(AppPermission.AccessAdminPanel)));

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Access denied. Admin privileges required.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await user.EnsureAllowedToLoginAsync(_identityContext, utcNow, cancellationToken);

        user.UpdateLastLogin(utcNow);

        var permissions = _permissionService.CalculateUserPermissions(user, utcNow);

        var token = _jwtTokenGenerator.GenerateToken(user, "admin");
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            hashedRefreshToken,
            utcNow.AddHours(12), 
            utcNow            
        );

        _identityContext.Add(refreshTokenEntity);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            user.Id,
            token,
            rawRefreshToken, 
            user.Email,
            permissions
        );
    }
}
