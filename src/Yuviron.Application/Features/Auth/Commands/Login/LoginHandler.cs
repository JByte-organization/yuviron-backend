using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()                 
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Subscriptions)            
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(user.Id, token, user.Email);
    }
}