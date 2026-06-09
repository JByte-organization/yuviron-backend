using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdatePrivacyToggles;

public class UpdatePrivacyTogglesCommandHandler : IRequestHandler<UpdatePrivacyTogglesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePrivacyTogglesCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdatePrivacyTogglesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);
        if (settings == null) throw new NotFoundException(nameof(UserSettings), userId);

        settings.UpdatePrivacy(request.MakePlaylistsPublicByDefault, request.ShowFollowers, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
