using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Policies;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateAudioQuality;

public sealed class UpdateAudioQualityCommandHandler : IRequestHandler<UpdateAudioQualityCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly UserSettingsPolicy _policy;

    public UpdateAudioQualityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPermissionService permissionService,
        UserSettingsPolicy policy)
    {
        _context = context;
        _currentUser = currentUser;
        _permissionService = permissionService;
        _policy = policy;
    }

    public async Task<Unit> Handle(UpdateAudioQualityCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);
        if (settings == null) throw new NotFoundException(nameof(UserSettings), userId);

        var hasHighQuality = await _permissionService.HasPermissionAsync(userId, AppPermission.PlayerHighQuality, cancellationToken);
        var sanitizedQuality = _policy.SanitizeAudioQuality(hasHighQuality, request.AudioQualityPreference);

        settings.UpdateAudioQuality(sanitizedQuality, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
