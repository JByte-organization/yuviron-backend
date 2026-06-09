using MediatR;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetCustomTheme;

public sealed record GetCustomThemeQuery : IRequest<CustomThemeDto>;
