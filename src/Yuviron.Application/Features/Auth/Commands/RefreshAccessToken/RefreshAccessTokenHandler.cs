using MediatR;
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
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider; 
    private readonly ILogger<RefreshAccessTokenHandler> _logger;

    public RefreshAccessTokenHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        TimeProvider timeProvider,
        ILogger<RefreshAccessTokenHandler> logger) 
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider; 
        _logger = logger;
    }

    public async Task<RefreshAccessTokenResponse> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var requestTokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken.Trim());

        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .Include(rt => rt.User.Subscriptions)
            .FirstOrDefaultAsync(rt => rt.TokenHash == requestTokenHash, cancellationToken); 

        if (existingToken == null) throw new UnauthorizedAccessException("Invalid token.");

        if (existingToken.User.AccountState == AccountState.Banned || existingToken.User.AccountState == AccountState.Deleted)
        {
            throw new UnauthorizedAccessException("This account has been banned or deleted.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        bool isRetryWithinGracePeriod = false;

        if (existingToken.IsRevoked)
        {
            if (existingToken.IsInGracePeriod(utcNow))
            {
                isRetryWithinGracePeriod = true;
                _logger.LogInformation("Grace period utilized for User {UserId}. Token reused within 1 minute.", existingToken.UserId);
            }
            else
            {
                var allUserTokens = await _context.RefreshTokens
                    .Where(x => x.UserId == existingToken.UserId && x.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var token in allUserTokens)
                {
                    token.Revoke(utcNow);
                }

                existingToken.User.AddDomainEvent(new UserPermissionsChangedEvent(existingToken.UserId));
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogWarning("Security Alert: Token reuse detected for User {UserId}. All sessions terminated.", existingToken.UserId);
                throw new UnauthorizedAccessException("Security Alert: Token reuse detected. All sessions terminated.");
            }
        }

        if (existingToken.IsExpired(utcNow)) 
        {
            throw new UnauthorizedAccessException("Token expired."); 
        }

        if (!isRetryWithinGracePeriod)
        {
            existingToken.Revoke(utcNow); 
        }

        var newAccessToken = _jwtTokenGenerator.GenerateToken(existingToken.User);
        var newRawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var newHashedRefreshToken = _jwtTokenGenerator.HashRefreshToken(newRawRefreshToken);

        var newRefreshTokenEntity = RefreshToken.Create(
            existingToken.UserId,
            newHashedRefreshToken, 
            utcNow.AddDays(30), 
            utcNow             
        );

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshAccessTokenResponse(newAccessToken, newRawRefreshToken);
    }
}