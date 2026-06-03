using MediatR;

namespace Yuviron.Application.Features.Client.Settings.Queries.GetMyDevices;

public sealed record GetMyDevicesQuery : IRequest<List<UserDeviceDto>>;