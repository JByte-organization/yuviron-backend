using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Client.Settings.Queries.Commands.RevokeDevice;

public sealed class RevokeDeviceHandler : IRequestHandler<RevokeDeviceCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;

    public RevokeDeviceHandler(IIdentityContext identityContext, ICurrentUserService currentUser)
    {
        _identityContext = identityContext; _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RevokeDeviceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var device = await _identityContext.UserDevices
                         .FirstOrDefaultAsync(d => d.Id == request.DeviceId && d.UserId == userId, cancellationToken)
                     ?? throw new NotFoundException("Device", request.DeviceId);

        _identityContext.Remove(device);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}