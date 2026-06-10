using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Themes.Commands.UpdateTheme;

public sealed record UpdateThemeCommand(
    Guid ThemeId,
    string Name,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    bool IsPremiumOnly,
    Guid? UserId) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
