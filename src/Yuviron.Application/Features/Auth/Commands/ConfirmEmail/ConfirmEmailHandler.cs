using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Application.Features.Auth.Commands.ConfirmEmail;

public sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly IOtpService _otpService;
    private readonly TimeProvider _timeProvider;

    public ConfirmEmailHandler(
        IIdentityContext identityContext, 
        IOtpService otpService, 
        TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _otpService = otpService;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        // 1. Ищем email в Redis по токену
        var email = await _otpService.GetEmailByConfirmationTokenAsync(request.Token, cancellationToken);
        
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("Link has expired or is invalid.");
        }

        // 2. Достаем юзера из базы (обязательно Include Profile, чтобы взять FirstName для письма!)
        var user = await _identityContext.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            
        if (user == null) 
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // 3. Если почта уже подтверждена - просто возвращаем ОК (Идемпотентность)
        if (user.IsEmailConfirmed)
        {
            return Unit.Value;
        }

        // 4. Подтверждаем почту (здесь внутри вызовется AddDomainEvent с UserEmailConfirmedEvent)
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        user.ConfirmEmail(user.Profile.FirstName, utcNow);
        
        await _identityContext.SaveChangesAsync(cancellationToken);

        // 5. Удаляем токен из Redis, чтобы он стал одноразовым
        await _otpService.RemoveConfirmationTokenAsync(request.Token, cancellationToken);

        return Unit.Value;
    }
}