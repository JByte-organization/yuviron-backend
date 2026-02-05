using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Auth.Queries.CheckEmail;

public class CheckEmailHandler : IRequestHandler<CheckEmailQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public CheckEmailHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
    }
}