using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Services;

public class UserDeviceTracker : IUserDeviceTracker
{
    private readonly IApplicationDbContext _context;
    private readonly IEventBus _eventBus;

    public UserDeviceTracker(IApplicationDbContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public async Task TrackDeviceAsync(
        Guid userId, 
        ClientContext clientInfo, 
        DateTime utcNow, 
        CancellationToken cancellationToken)
    {
        var knownDevice = await _context.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId 
                                      && d.DeviceName == clientInfo.Device 
                                      && d.BrowserName == clientInfo.Browser, cancellationToken);

        bool isNewDevice = false;

        if (knownDevice == null)
        {
            isNewDevice = true;
            var newDevice = UserDevice.Create(userId, clientInfo.Device, clientInfo.Browser, clientInfo.IpAddress, utcNow);
            _context.UserDevices.Add(newDevice);
        }
        else
        {
            knownDevice.UpdateLastUsed(clientInfo.IpAddress, utcNow);
        }

        // 1. СНАЧАЛА сохраняем данные в БД (это зафиксирует изменения для теста и для реальности)
        await _context.SaveChangesAsync(cancellationToken);

        // 2. ПОТОМ отправляем событие. 
        // Если SaveChangesAsync упадет, код дальше не пойдет, и событие не отправится.
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