using MediatR;

namespace Yuviron.Application.Features.Client.Appearance.Commands.DeleteThemePreset;

public sealed record DeleteThemePresetCommand(Guid Id) : IRequest<Unit>;
