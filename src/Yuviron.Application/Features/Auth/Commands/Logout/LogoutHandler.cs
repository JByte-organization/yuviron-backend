using MediatR;
using Microsoft.EntityFrameworkCore;
using System; // <-- Добавлено
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Application.Features.Auth.Commands.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly TimeProvider _timeProvider; 

    public LogoutHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator, TimeProvider timeProvider) 
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (token != null)
        {
            token.Revoke(_timeProvider.GetUtcNow().UtcDateTime);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}