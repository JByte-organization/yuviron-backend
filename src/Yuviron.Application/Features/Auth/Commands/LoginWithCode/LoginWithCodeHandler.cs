using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Identity; 
using Yuviron.Application.Abstractions.Services; // <-- IUserDeviceTracker
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public sealed class LoginWithCodeHandler : IRequestHandler<LoginWithCodeCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IOtpService _otpService;
    private readonly IPermissionService _permissionService; 
    private readonly IClientContextService _clientContextService;
    private readonly IUserDeviceTracker _deviceTracker; 

    public LoginWithCodeHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        IOtpService otpService,
        IPermissionService permissionService,
        IClientContextService clientContextService,
        IUserDeviceTracker deviceTracker) 
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
        _otpService = otpService;
        _permissionService = permissionService;
        _clientContextService = clientContextService;
        _deviceTracker = deviceTracker;
    }

    public async Task<LoginResponse> Handle(LoginWithCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var cachedCodeHash = await _otpService.GetLoginCodeHashAsync(normalizedEmail, cancellationToken);
        if (string.IsNullOrEmpty(cachedCodeHash)) throw new UnauthorizedAccessException("Code expired or not found.");
        if (!_passwordHasher.Verify(request.Code, cachedCodeHash)) throw new UnauthorizedAccessException("Invalid code.");

        await _otpService.RemoveLoginCodeAsync(normalizedEmail, cancellationToken);

        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("User not found.");
        if (user.AccountState == AccountState.Deleted) throw new UnauthorizedAccessException("This account has been deleted.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        await user.EnsureAllowedToLoginAsync(_context, utcNow, cancellationToken);
        user.UpdateLastLogin(utcNow);

        var clientInfo = _clientContextService.GetClientContext();
        await _deviceTracker.TrackDeviceAsync(user.Id, clientInfo, utcNow, cancellationToken);

        var permissions = _permissionService.CalculateUserPermissions(user, utcNow);
        var token = _jwtTokenGenerator.GenerateToken(user);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(user.Id, hashedRefreshToken, utcNow.AddDays(30), utcNow);
        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(user.Id, token, rawRefreshToken, user.Email, permissions);
    }
}