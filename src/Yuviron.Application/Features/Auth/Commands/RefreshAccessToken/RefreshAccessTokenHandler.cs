using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Auth.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenHandler : IRequestHandler<RefreshAccessTokenCommand, RefreshAccessTokenResponse>
{
    private readonly IIdentityContext _identityContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider; 
    private readonly ILogger<RefreshAccessTokenHandler> _logger;

    public RefreshAccessTokenHandler(
        IIdentityContext identityContext,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        ILogger<RefreshAccessTokenHandler> logger) 
    {
        _identityContext = identityContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider; 
        _logger = logger;
    }

    public async Task<RefreshAccessTokenResponse> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var requestTokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken.Trim());

        var existingToken = await _identityContext.RefreshTokens
            .IgnoreQueryFilters() 
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(rt => rt.User.Subscriptions)
            .FirstOrDefaultAsync(rt => rt.TokenHash == requestTokenHash, cancellationToken); 

        if (existingToken == null) throw new UnauthorizedAccessException("Invalid token.");

        if (existingToken.User == null) throw new UnauthorizedAccessException("User not found.");

        if (existingToken.User.AccountState == AccountState.Banned || 
            existingToken.User.AccountState == AccountState.Deleted)
        {
            throw new UnauthorizedAccessException("This account has been banned or deleted.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (existingToken.IsRevoked)
        {
            if (existingToken.IsInGracePeriod(utcNow))
            {
                _logger.LogInformation("Network retrace! Grace period for user token {UserId}.", existingToken.UserId);

                if (existingToken.RevokedAt.HasValue)
                {
                    var lostTokens = await _identityContext.RefreshTokens
                        .Where(t => t.UserId == existingToken.UserId 
                                 && t.CreatedAt >= existingToken.RevokedAt.Value 
                                 && t.RevokedAt == null)
                        .ToListAsync(cancellationToken);

                    foreach (var lostToken in lostTokens)
                    {
                        lostToken.Revoke(utcNow);
                    }
                }
                
            }
            else
            {
                var allUserTokens = await _identityContext.RefreshTokens
                    .Where(x => x.UserId == existingToken.UserId && x.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var token in allUserTokens) token.Revoke(utcNow);

                existingToken.User.AddDomainEvent(new UserPermissionsChangedEvent(existingToken.UserId));
                await _identityContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogWarning("Security Alert: Token reuse detected for User {UserId}. All sessions terminated.", existingToken.UserId);
                throw new UnauthorizedAccessException("Security Alert: Token reuse detected. All sessions terminated.");
            }
        }
        else if (existingToken.IsExpired(utcNow)) 
        {
            throw new UnauthorizedAccessException("Token expired."); 
        }
        else 
        {
            existingToken.Revoke(utcNow);
        }

        
        bool isAdmin = existingToken.User.UserRoles.Any(ur => 
            ur.Role.RolePermissions.Any(rp => rp.Permission.Name == nameof(AppPermission.AccessAdminPanel)));

        var tokenTtl = isAdmin ? TimeSpan.FromHours(12) : TimeSpan.FromDays(30);


        var newAccessToken = _jwtTokenGenerator.GenerateToken(existingToken.User);
        var newRawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var newHashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(newRawRefreshToken);

        var newRefreshTokenEntity = RefreshToken.Create(
            existingToken.UserId,
            newHashedRefreshToken, 
            utcNow.Add(tokenTtl),
            utcNow             
        );

        _identityContext.Add(newRefreshTokenEntity);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return new RefreshAccessTokenResponse(newAccessToken, newRawRefreshToken);
    }
}
