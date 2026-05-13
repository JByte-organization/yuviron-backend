using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public ResetPasswordHandler(
        IApplicationDbContext context, 
        IOtpService otpService, 
        IPasswordHasher passwordHasher, 
        TimeProvider timeProvider)
    {
        _context = context;
        _otpService = otpService;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Пытаемся достать email по токену из Redis
        var email = await _otpService.GetEmailByPasswordResetTokenAsync(request.Token, cancellationToken);
        
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("Посилання недійсне або його термін дії минув.");
        }

        // 2. Ищем пользователя
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null) 
        {
            throw new UnauthorizedAccessException("Користувача не знайдено.");
        }

        // 3. Хешируем новый пароль и обновляем (метод SetPasswordHash также кинет событие сброса токенов!)
        var newHash = _passwordHasher.Hash(request.NewPassword);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        user.SetPasswordHash(newHash, utcNow);

        // 4. Сохраняем изменения
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Обязательно удаляем токен, чтобы его нельзя было использовать повторно
        await _otpService.RemovePasswordResetTokenAsync(request.Token, cancellationToken);

        return Unit.Value;
    }
}