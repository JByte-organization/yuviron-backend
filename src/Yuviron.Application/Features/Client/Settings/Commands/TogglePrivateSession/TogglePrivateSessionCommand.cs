using MediatR;
namespace Yuviron.Application.Features.Client.Settings.Commands.TogglePrivateSession;
public record TogglePrivateSessionCommand(bool PrivateSession) : IRequest<Unit>;
