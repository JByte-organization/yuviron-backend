using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Auth.Commands.Logout;


public sealed class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogoutHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenGenerator.HashRefreshToken(request.RefreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (token != null)
        {
            token.Revoke(_dateTimeProvider.UtcNow);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}