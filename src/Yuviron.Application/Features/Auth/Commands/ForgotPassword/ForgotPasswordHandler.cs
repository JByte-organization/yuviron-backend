using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Domain.Common;

namespace Yuviron.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly IOtpService _otpService;
    private readonly TimeProvider _timeProvider;

    public ForgotPasswordHandler(
        IIdentityContext identityContext, 
        IOtpService otpService, 
        TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _otpService = otpService;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var user = await _identityContext.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null) 
        {
            await Task.Delay(Random.Shared.Next(150, 300), cancellationToken);
            return Unit.Value;
        }

        var token = Guid.NewGuid().ToString("N");
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await _otpService.SavePasswordResetTokenAsync(token, normalizedEmail, TimeSpan.FromHours(1), cancellationToken);

        user.RequestPasswordReset(token, user.Profile.FirstName, utcNow);
        
        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}