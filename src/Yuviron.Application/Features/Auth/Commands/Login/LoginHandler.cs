using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Common;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService;

    public LoginHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        IPermissionService permissionService) 
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var user = await _context.Users
             .Include(u => u.UserRoles)
                 .ThenInclude(ur => ur.Role)
                     .ThenInclude(r => r.RolePermissions)
                         .ThenInclude(rp => rp.Permission)
             .Include(u => u.Subscriptions)
             .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("Invalid credentials.");

        bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) throw new UnauthorizedAccessException("Invalid credentials.");

        if (user.AccountState == AccountState.Banned || user.AccountState == AccountState.Deleted)
        {
            throw new UnauthorizedAccessException("This account has been banned or deleted.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        user.UpdateLastLogin(utcNow);

        var permissions = _permissionService.CalculateUserPermissions(user, utcNow);

        var token = _jwtTokenGenerator.GenerateToken(user);
        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(
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