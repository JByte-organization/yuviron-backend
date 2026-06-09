using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetThemeModes;

public sealed record GetThemeModesQuery : IRequest<List<ThemeMode>>;
