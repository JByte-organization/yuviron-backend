using MediatR;

namespace Yuviron.Application.Features.Client.Artists.Commands.ToggleNotifications;

public sealed record ToggleArtistNotificationsCommand(
    Guid ArtistId, 
    bool ReceiveNotifications
) : IRequest<Unit>;