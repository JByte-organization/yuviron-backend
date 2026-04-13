using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Common;

namespace Yuviron.Application.Features.Auth.Queries.CheckEmail;

public sealed class CheckEmailHandler : IRequestHandler<CheckEmailQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public CheckEmailHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
}
