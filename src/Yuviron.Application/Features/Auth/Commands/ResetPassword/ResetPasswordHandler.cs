using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public ResetPasswordHandler(
        IApplicationDbContext context, 
        IOtpService otpService, 
        IPasswordHasher passwordHasher, 
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _context = context;
        _otpService = otpService;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = await _otpService.GetEmailByPasswordResetTokenAsync(request.Token, cancellationToken);
        
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("The message is ineffective or its term has passed.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null) 
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        user.SetPasswordHash(newHash, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserPasswordResetCompletedEvent(user.Id), cancellationToken);
        
        await _otpService.RemovePasswordResetTokenAsync(request.Token, cancellationToken);

        return Unit.Value;
    }
}
