using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Finance.Commands.UpdatePayoutSettings;

public sealed class UpdatePayoutSettingsHandler : IRequestHandler<UpdatePayoutSettingsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;

    public UpdatePayoutSettingsHandler(IApplicationDbContext context, ICurrentUserService currentUser, TimeProvider timeProvider)
    {
        _context = context; _currentUser = currentUser; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdatePayoutSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var isOwner = await _context.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId && tm.Role == ArtistTeamRole.Owner, cancellationToken);
        
        if (!isOwner) throw new ForbiddenException("Only the Owner can update payout settings.");

        var settings = await _context.ArtistPayoutSettings
            .FirstOrDefaultAsync(s => s.ArtistId == request.ArtistId, cancellationToken);

        if (settings == null)
        {
            settings = ArtistPayoutSettings.Create(request.ArtistId, 50m, 5000m, 30, request.Method, request.AccountDetails, null, utcNow);
            _context.ArtistPayoutSettings.Add(settings);
        }
        else
        {
            settings.UpdatePayoutMethod(request.Method, request.AccountDetails, utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}