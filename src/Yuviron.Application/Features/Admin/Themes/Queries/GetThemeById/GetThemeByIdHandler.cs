using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Themes.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Queries.GetThemeById;

public sealed class GetThemeByIdHandler : IRequestHandler<GetThemeByIdQuery, ThemeDetailsDto>
{
    private readonly IProfileContext _profileContext;

    public GetThemeByIdHandler(IProfileContext profileContext)
    {
        _profileContext = profileContext;
    }

    public async Task<ThemeDetailsDto> Handle(GetThemeByIdQuery request, CancellationToken cancellationToken)
    {
        var theme = await _profileContext.Themes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.Id);

        return new ThemeDetailsDto(
            theme.Id,
            theme.Name,
            theme.PrimaryColor,
            theme.SecondaryColor,
            theme.BackgroundColor,
            theme.IsSystem,
            theme.IsPremiumOnly,
            theme.UserId);
    }
}
