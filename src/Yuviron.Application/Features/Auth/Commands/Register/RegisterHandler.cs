using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService; 

    public RegisterHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IEmailService emailService) 
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            throw new UserAlreadyExistsException(request.Email);
        }

        
        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash);
        var profile = UserProfile.Create(user.Id, request.FirstName);
        user.SetProfile(profile);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

       try
        {
            await _emailService.SendEmailAsync(
                user.Email,
                "Добро пожаловать в Yuviron!",
                $"<h1>Привет, {request.FirstName}!</h1><p>Спасибо за регистрацию.</p>",
                cancellationToken);
        }
        catch
        {
            
        }

        return user.Id;
    }
}