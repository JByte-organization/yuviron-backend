using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Themes.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Themes.Queries.GetThemeById;

public sealed record GetThemeByIdQuery(Guid Id) : IRequest<ThemeDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
