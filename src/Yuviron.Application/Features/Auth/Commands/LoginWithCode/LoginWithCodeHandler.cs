using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq; 
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Auth.Commands.Login;
using Yuviron.Domain.Common;
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

    public LoginWithCodeHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        IOtpService otpService,
        IPermissionService permissionService) 
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
        _otpService = otpService;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> Handle(LoginWithCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var cachedCodeHash = await _otpService.GetLoginCodeHashAsync(normalizedEmail, cancellationToken);
        
        if (string.IsNullOrEmpty(cachedCodeHash))
        {
            throw new UnauthorizedAccessException("Code expired or not found.");
        }

        if (!_passwordHasher.Verify(request.Code, cachedCodeHash))
        {
            throw new UnauthorizedAccessException("Invalid code.");
        }

        await _otpService.RemoveLoginCodeAsync(normalizedEmail, cancellationToken);

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("User not found.");

        if (user.AccountState == AccountState.Deleted)
        {
            throw new UnauthorizedAccessException("This account has been deleted.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (user.AccountState == AccountState.Banned)
        {
            var activeBlocks = await _context.UserBlocks
                .Where(b => b.UserId == user.Id && b.IsActive)
                .ToListAsync(cancellationToken);

            bool isStillBanned = activeBlocks.Any(b => b.EndsAt == null || b.EndsAt > utcNow);

            if (isStillBanned)
            {
                throw new UnauthorizedAccessException("This account is currently banned.");
            }

            foreach (var block in activeBlocks)
            {
                block.Deactivate(utcNow);
            }

            user.SetAccountState(AccountState.Active, utcNow);
        }

        user.UpdateLastLogin(utcNow);

        var permissions = _permissionService.CalculateUserPermissions(user, utcNow);

        var token = _jwtTokenGenerator.GenerateToken(user);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = Yuviron.Domain.Entities.RefreshToken.Create(
            user.Id,
            hashedRefreshToken,
            utcNow.AddDays(30), 
            utcNow            
        );

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            user.Id,
            token,
            rawRefreshToken, 
            user.Email,
            permissions
        );
    }
}