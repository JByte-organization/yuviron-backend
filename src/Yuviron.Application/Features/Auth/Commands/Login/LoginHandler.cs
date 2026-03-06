using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Common;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPermissionService _permissionService;
    private readonly TimeProvider _timeProvider;

    public LoginHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IPermissionService permissionService,
        TimeProvider timeProvider) 
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _permissionService = permissionService;
        _timeProvider = timeProvider;
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

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        user.UpdateLastLogin(utcNow);

        var permissions = await _permissionService.CachePermissionsAsync(user, cancellationToken);

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