using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Domain.Common;

namespace Yuviron.Application.Features.Auth.Commands.SendLoginCode;

public sealed class SendLoginCodeHandler : IRequestHandler<SendLoginCodeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SendLoginCodeHandler> _logger;
    private readonly IOtpService _otpService;

    public SendLoginCodeHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        ILogger<SendLoginCodeHandler> logger,
        IOtpService otpService) 
    {
        _context = context;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _otpService = otpService; 
    }

    public async Task<Unit> Handle(SendLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (!userExists)
        {
            return Unit.Value;
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var codeHash = _passwordHasher.Hash(code);

        await _otpService.SaveLoginCodeHashAsync(
            normalizedEmail, 
            codeHash, 
            TimeSpan.FromMinutes(10), 
            cancellationToken);

        try
        {
            await _emailService.SendEmailAsync(
                normalizedEmail,
                "Yuviron Login Code",
                $"<h1>{code}</h1>",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send login code to {Email}", normalizedEmail);
        }

        return Unit.Value;
    }
}