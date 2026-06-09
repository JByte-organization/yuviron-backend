using MediatR;

namespace Yuviron.Application.Features.Client.Appearance.Commands.UpdateThemePreset;

public sealed record UpdateThemePresetCommand(
    Guid Id,
    string Name,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor) : IRequest<Unit>;
