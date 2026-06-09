using MediatR;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetThemes;

public sealed record GetThemesQuery : IRequest<List<ThemeDto>>;
