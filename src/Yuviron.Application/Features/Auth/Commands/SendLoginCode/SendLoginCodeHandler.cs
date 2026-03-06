using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Auth.Commands.SendLoginCode;

public sealed class SendLoginCodeHandler : IRequestHandler<SendLoginCodeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SendLoginCodeHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public SendLoginCodeHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        ILogger<SendLoginCodeHandler> logger,
        TimeProvider timeProvider) 
    {
        _context = context;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _timeProvider = timeProvider; 
    }

    public async Task<Unit> Handle(SendLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null)
        {
            return Unit.Value;
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var codeHash = _passwordHasher.Hash(code);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        user.SetLoginCode(codeHash, utcNow); 
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Yuviron Login Code",
                $"<h1>{code}</h1>",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send login code to {Email}", user.Email);
        }

        return Unit.Value;
    }
}