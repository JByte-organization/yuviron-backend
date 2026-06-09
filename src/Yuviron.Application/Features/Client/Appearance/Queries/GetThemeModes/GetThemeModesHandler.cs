using MediatR;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Appearance.Queries.GetThemeModes;

public sealed class GetThemeModesHandler : IRequestHandler<GetThemeModesQuery, List<ThemeMode>>
{
    public async Task<List<ThemeMode>> Handle(GetThemeModesQuery request, CancellationToken cancellationToken)
    {
        return Enum.GetValues<ThemeMode>().ToList();
    }
}
