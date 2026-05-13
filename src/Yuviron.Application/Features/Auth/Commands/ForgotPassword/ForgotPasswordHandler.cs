using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Common;

namespace Yuviron.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly TimeProvider _timeProvider;

    public ForgotPasswordHandler(
        IApplicationDbContext context, 
        IOtpService otpService, 
        TimeProvider timeProvider)
    {
        _context = context;
        _otpService = otpService;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null) return Unit.Value;

        var token = Guid.NewGuid().ToString("N");
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _otpService.SavePasswordResetTokenAsync(token, normalizedEmail, TimeSpan.FromHours(1), cancellationToken);

        user.RequestPasswordReset(token, user.Profile.FirstName, utcNow);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}