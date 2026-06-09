using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateCrossfade;

public sealed class UpdateCrossfadeCommandHandler : IRequestHandler<UpdateCrossfadeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCrossfadeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateCrossfadeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);
        if (settings == null) throw new NotFoundException(nameof(UserSettings), userId);

        settings.UpdateCrossfade(request.CrossfadeMs, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
