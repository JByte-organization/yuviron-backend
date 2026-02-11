using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services; // Для IDateTimeProvider
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;

public class RefreshAccessTokenHandler : IRequestHandler<RefreshAccessTokenCommand, RefreshAccessTokenResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPermissionService _permissionService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshAccessTokenHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IPermissionService permissionService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _permissionService = permissionService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RefreshAccessTokenResponse> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var requestTokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);

        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .Include(rt => rt.User.Subscriptions)
            .FirstOrDefaultAsync(rt => rt.TokenHash == requestTokenHash, cancellationToken); 

        if (existingToken == null) throw new UnauthorizedAccessException("Invalid token.");

        if (existingToken.RevokedAt != null) throw new UnauthorizedAccessException("Token is revoked.");

        if (existingToken.ExpiresAt < _dateTimeProvider.UtcNow) throw new UnauthorizedAccessException("Token expired.");

        existingToken.Revoke();

        var newAccessToken = _jwtTokenGenerator.GenerateToken(existingToken.User);

        var newRawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var newHashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(newRawRefreshToken);

        var newRefreshTokenEntity = RefreshToken.Create(
            existingToken.UserId,
            newHashedRefreshToken, 
            _dateTimeProvider.UtcNow.AddDays(30)
        );

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        await _permissionService.CachePermissionsAsync(existingToken.User, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshAccessTokenResponse(newAccessToken, newRawRefreshToken);
    }
}