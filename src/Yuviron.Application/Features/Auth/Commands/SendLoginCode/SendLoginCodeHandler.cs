using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;

namespace Yuviron.Application.Features.Auth.Commands.SendLoginCode;

public class SendLoginCodeHandler : IRequestHandler<SendLoginCodeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;

    public SendLoginCodeHandler(IApplicationDbContext context, IEmailService emailService, IPasswordHasher passwordHasher)
    {
        _context = context;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(SendLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null) return Unit.Value;

        var code = Random.Shared.Next(100000, 999999).ToString();
        var codeHash = _passwordHasher.Hash(code);

        user.SetLoginCode(codeHash);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Yuviron Login Code",
                $"<h1>{code}</h1>",
                cancellationToken);
        }
        catch {  }

        return Unit.Value;
    }
}