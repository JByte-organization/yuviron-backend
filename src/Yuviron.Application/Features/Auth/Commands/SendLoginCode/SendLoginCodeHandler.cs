using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Common;

namespace Yuviron.Application.Features.Auth.Commands.SendLoginCode;

public record LoginCodeModel(string Code);
public record AccountNotFoundModel(string Email); 

public sealed class SendLoginCodeHandler : IRequestHandler<SendLoginCodeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SendLoginCodeHandler> _logger;
    private readonly IOtpService _otpService;
    private readonly ITemplateService _templateService;

    public SendLoginCodeHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        ILogger<SendLoginCodeHandler> logger,
        IOtpService otpService,
        ITemplateService templateService)
    {
        _context = context;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _otpService = otpService;
        _templateService = templateService;
    }

    public async Task<Unit> Handle(SendLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (!userExists)
        {
            await Task.Delay(Random.Shared.Next(400, 800), cancellationToken);

            try
            {
                var htmlBody = await _templateService.RenderTemplateAsync("AccountNotFound", new AccountNotFoundModel(normalizedEmail));
                
                await _emailService.SendEmailAsync(
                    normalizedEmail,
                    "Yuviron - Login Attempt",
                    htmlBody,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send AccountNotFound notice to {Email}", normalizedEmail);
            }

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
            var htmlBody = await _templateService.RenderTemplateAsync("LoginCode", new LoginCodeModel(code));

            await _emailService.SendEmailAsync(
                normalizedEmail,
                "Yuviron - Your Login Code",
                htmlBody,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send login code to {Email}", normalizedEmail);
        }

        return Unit.Value;
    }
}