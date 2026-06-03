using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;

public sealed class GetMyDevicesHandler : IRequestHandler<GetMyDevicesQuery, List<UserDeviceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyDevicesHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context; _currentUser = currentUser;
    }

    public async Task<List<UserDeviceDto>> Handle(GetMyDevicesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        return await _context.UserDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastUsedAt)
            .Select(d => new UserDeviceDto(d.Id, d.DeviceName, d.BrowserName, d.LastIpAddress, d.LastUsedAt, d.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}