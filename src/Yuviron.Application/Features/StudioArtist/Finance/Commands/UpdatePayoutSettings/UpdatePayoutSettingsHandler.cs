using Yuviron.Application.Abstractions.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Finance.Commands.UpdatePayoutSettings;

public sealed class UpdatePayoutSettingsHandler : IRequestHandler<UpdatePayoutSettingsCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly IMonetizationContext _monetizationContext;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public UpdatePayoutSettingsHandler(
        ICatalogContext catalogContext, IMonetizationContext monetizationContext, 
        ICurrentUserService currentUser, 
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _catalogContext = catalogContext;
        _monetizationContext = monetizationContext; _currentUser = currentUser; _timeProvider = timeProvider; _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdatePayoutSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var isOwner = await _catalogContext.ArtistTeamMembers
            .AnyAsync(tm => tm.ArtistId == request.ArtistId && tm.UserId == userId && tm.Role == ArtistTeamRole.Owner, cancellationToken);
        
        if (!isOwner) throw new ForbiddenException("Only the Owner can update payout settings.");

        var settings = await _monetizationContext.ArtistPayoutSettings
            .FirstOrDefaultAsync(s => s.ArtistId == request.ArtistId, cancellationToken);

        if (settings == null)
        {
            settings = ArtistPayoutSettings.Create(request.ArtistId, 50m, 5000m, 30, request.Method, request.AccountDetails, null, utcNow);
            _monetizationContext.Add(settings);
        }
        else
        {
            settings.UpdatePayoutMethod(request.Method, request.AccountDetails, utcNow);
        }

        await _catalogContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new PayoutSettingsChangedEvent(request.ArtistId), cancellationToken);

        return Unit.Value;
    }
}