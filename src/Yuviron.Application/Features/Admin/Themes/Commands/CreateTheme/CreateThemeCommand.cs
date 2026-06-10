using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Themes.Commands.CreateTheme;

public sealed record CreateThemeCommand(
    string Name,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    bool IsPremiumOnly,
    Guid? UserId) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
