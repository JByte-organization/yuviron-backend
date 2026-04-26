using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Auth.Commands.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider; 
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IApplicationDbContext context, 
        IJwtTokenGenerator jwtTokenGenerator, 
        TimeProvider timeProvider,
        ICurrentUserService currentUserService,
        ILogger<LogoutHandler> logger) 
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        var tokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (token != null)
        {
            if (token.UserId != currentUserId)
            {
                _logger.LogWarning("Security Alert: User {AttackerId} attempted to revoke a refresh token belonging to User {VictimId}", currentUserId, token.UserId);
                
                return Unit.Value; 
            }

            token.Revoke(_timeProvider.GetUtcNow().UtcDateTime);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}