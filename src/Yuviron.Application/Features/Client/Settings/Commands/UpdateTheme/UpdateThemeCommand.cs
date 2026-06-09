using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Settings.Commands.UpdateTheme;

public record UpdateThemeCommand(ThemeMode ThemeMode) : IRequest<Unit>;
