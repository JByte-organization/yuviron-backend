using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Services;

public class UserDeviceTracker : IUserDeviceTracker
{
    private readonly AppDbContext _identityContext;
    private readonly IEventBus _eventBus;

    public UserDeviceTracker(AppDbContext identityContext, IEventBus eventBus)
    {
        _identityContext = identityContext;
        _eventBus = eventBus;
    }

    public async Task TrackDeviceAsync(
        Guid userId, 
        ClientContext clientInfo, 
        DateTime utcNow, 
        CancellationToken cancellationToken)
    {
        var knownDevice = await _identityContext.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId 
                                      && d.Fingerprint == clientInfo.Fingerprint, cancellationToken);

        bool isNewDevice = false;

        if (knownDevice == null)
        {
            isNewDevice = true;
            var newDevice = UserDevice.Create(
                userId, 
                clientInfo.Fingerprint, 
                clientInfo.Device, 
                clientInfo.Browser, 
                clientInfo.IpAddress, 
                utcNow);
                
            _identityContext.Add(newDevice);
        }
        else
        {
            knownDevice.UpdateLastUsed(clientInfo.IpAddress, utcNow);
        }

        await _identityContext.SaveChangesAsync(cancellationToken);

        if (isNewDevice)
        {
            await _eventBus.PublishAsync(new NewDeviceLoginEvent(
                userId,
                clientInfo.Device,
                clientInfo.Browser,
                clientInfo.IpAddress
            ), cancellationToken);
        }
    }
}