using MediatR;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateCustomTheme;

public sealed record UpdateCustomThemeCommand(
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor
) : IRequest<Unit>;
