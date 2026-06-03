using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Identity;

namespace Yuviron.Application.Abstractions.Services;

public interface IUserDeviceTracker
{
    Task TrackDeviceAsync(
        Guid userId, 
        ClientContext clientInfo, 
        DateTime utcNow, 
        CancellationToken cancellationToken);
}