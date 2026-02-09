using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Features.Auth.Commands.Login; // Для LoginResponse
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public class LoginWithCodeHandler : IRequestHandler<LoginWithCodeCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPermissionService _permissionService;

    public LoginWithCodeHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IPermissionService permissionService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _permissionService = permissionService;
    }

    public async Task<LoginResponse> Handle(LoginWithCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("Invalid credentials.");

        if (user.LoginCodeExpiryUtc == null || user.LoginCodeExpiryUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Code expired.");
        }

        if (!_passwordHasher.Verify(request.Code, user.LoginCodeHash!))
        {
            throw new UnauthorizedAccessException("Invalid code.");
        }

        user.ClearLoginCode();

        var permissions = await _permissionService.CachePermissionsAsync(user, cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshTokenString,
            DateTime.UtcNow.AddDays(30)
        );

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            user.Id,
            token,
            refreshTokenString,
            user.Email,
            permissions 
        );
    }
}