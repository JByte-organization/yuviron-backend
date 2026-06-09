using MediatR;

namespace Yuviron.Application.Features.Client.Appearance.Commands.ActivateThemePreset;

public sealed record ActivateThemePresetCommand(Guid Id) : IRequest<Unit>;
