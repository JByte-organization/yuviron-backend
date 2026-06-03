using MediatR;

namespace Yuviron.Application.Features.Client.Settings.Queries.Commands.RevokeDevice;

public sealed record RevokeDeviceCommand(Guid DeviceId) : IRequest<Unit>;