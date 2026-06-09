using MediatR;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateAudioQuality;

public sealed record UpdateAudioQualityCommand(int AudioQualityPreference) : IRequest<Unit>;
