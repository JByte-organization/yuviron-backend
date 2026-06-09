using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateMarketingPreferences;

public class UpdateMarketingPreferencesCommandHandler : IRequestHandler<UpdateMarketingPreferencesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateMarketingPreferencesCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateMarketingPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new NotFoundException(nameof(User), userId);

        user.UpdateMarketingPreferences(request.AcceptMarketing, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
