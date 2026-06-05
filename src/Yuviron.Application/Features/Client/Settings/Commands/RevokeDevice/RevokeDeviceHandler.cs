using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Settings.Queries.Commands.RevokeDevice;

public sealed class RevokeDeviceHandler : IRequestHandler<RevokeDeviceCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RevokeDeviceHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RevokeDeviceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var device = await _context.UserDevices
                         .FirstOrDefaultAsync(d => d.Id == request.DeviceId && d.UserId == userId, cancellationToken)
                     ?? throw new NotFoundException("Device", request.DeviceId);

        _context.UserDevices.Remove(device);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}