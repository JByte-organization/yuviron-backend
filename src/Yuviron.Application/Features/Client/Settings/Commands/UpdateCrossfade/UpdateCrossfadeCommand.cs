using MediatR;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateCrossfade;

public sealed record UpdateCrossfadeCommand(int CrossfadeMs) : IRequest<Unit>;
