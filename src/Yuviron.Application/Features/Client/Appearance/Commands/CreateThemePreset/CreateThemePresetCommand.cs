using MediatR;

namespace Yuviron.Application.Features.Client.Appearance.Commands.CreateThemePreset;

public sealed record CreateThemePresetCommand(
    string Name,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor) : IRequest<Guid>;
