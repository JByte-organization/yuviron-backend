using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;

public sealed class GetMyDevicesHandler : IRequestHandler<GetMyDevicesQuery, List<UserDeviceDto>>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;

    public GetMyDevicesHandler(IIdentityContext identityContext, ICurrentUserService currentUser)
    {
        _identityContext = identityContext; _currentUser = currentUser;
    }

    public async Task<List<UserDeviceDto>> Handle(GetMyDevicesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var allDevices = await _identityContext.UserDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastUsedAt)
            .ToListAsync(cancellationToken);

        return allDevices
            .GroupBy(d => new { d.DeviceName, d.BrowserName })
            .Select(g => g.First())
            .Select(d => new UserDeviceDto(d.Id, d.DeviceName, d.BrowserName, d.LastIpAddress, d.LastUsedAt, d.CreatedAt))
            .ToList();
    }
}

