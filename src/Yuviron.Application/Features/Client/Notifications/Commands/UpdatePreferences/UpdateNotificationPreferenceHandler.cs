using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Features.Client.Notifications.Preferences;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Notifications.Commands.UpdatePreferences;

public sealed class UpdateNotificationPreferenceHandler : IRequestHandler<UpdateNotificationPreferenceCommand, Unit>
{
    private readonly IProfileContext _profileContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateNotificationPreferenceHandler(IProfileContext profileContext, ICurrentUserService currentUser)
    {
        _profileContext = profileContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var code = request.Code.Trim();
        var utcNow = DateTime.UtcNow;

        var existing = await _profileContext.UserNotificationPreferences
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Category == request.Category &&
                x.Code == code,
                cancellationToken);

        var defaultEnabled = NotificationPreferenceCatalog.IsDefaultEnabled(request.Category, code);

        if (request.Enabled == defaultEnabled)
        {
            if (existing != null)
            {
                _profileContext.Remove(existing);
            }

            await _profileContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        if (existing == null)
        {
            existing = UserNotificationPreference.Create(userId, request.Category, code, request.Enabled, utcNow);
            _profileContext.Add(existing);
        }
        else
        {
            existing.Update(request.Enabled, utcNow);
        }

        await _profileContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
