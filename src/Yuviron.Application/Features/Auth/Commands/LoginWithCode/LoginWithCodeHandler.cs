using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Features.Auth.Commands.Login;

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public class LoginWithCodeHandler : IRequestHandler<LoginWithCodeCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginWithCodeHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginWithCodeCommand request, CancellationToken cancellationToken)
    {
        // Нам нужны данные для токена, поэтому Include обязателен
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null) throw new UnauthorizedAccessException("Invalid credentials.");

        // Проверки
        if (user.LoginCodeExpiryUtc == null || user.LoginCodeExpiryUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Code expired.");
        }

        if (!_passwordHasher.Verify(request.Code, user.LoginCodeHash!))
        {
            throw new UnauthorizedAccessException("Invalid code.");
        }

        // Изменение состояния (очистка кода)
        user.ClearLoginCode();
        await _context.SaveChangesAsync(cancellationToken);

        // Генерация токена
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(user.Id, token, user.Email);
    }
}