using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;

    public UpdatePrivacyTogglesCommandHandler(IProfileContext profileContext, ICurrentUserService currentUser)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdatePrivacyTogglesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var settings = await _profileContext.UserSettings.FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);
        if (settings == null) throw new NotFoundException(nameof(UserSettings), userId);

        settings.UpdatePrivacy(request.MakePlaylistsPublicByDefault, request.ShowFollowers, DateTime.UtcNow);
        await _profileContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
