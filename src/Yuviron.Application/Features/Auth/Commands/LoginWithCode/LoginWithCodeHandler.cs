using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services; 
using Yuviron.Application.Features.Auth.Commands.Login; 
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public class LoginWithCodeHandler : IRequestHandler<LoginWithCodeCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPermissionService _permissionService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginWithCodeHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IPermissionService permissionService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _permissionService = permissionService;
        _dateTimeProvider = dateTimeProvider;
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

        if (user.LoginCodeExpiryUtc == null || user.LoginCodeExpiryUtc < _dateTimeProvider.UtcNow)
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

        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var hashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            hashedRefreshToken,
            _dateTimeProvider.UtcNow.AddDays(30)
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