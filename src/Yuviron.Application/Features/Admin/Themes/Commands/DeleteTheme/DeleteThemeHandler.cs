using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Commands.DeleteTheme;

public sealed class DeleteThemeHandler : IRequestHandler<DeleteThemeCommand, Unit>
{
    private readonly IProfileContext _profileContext;

    public DeleteThemeHandler(IProfileContext profileContext)
    {
        _profileContext = profileContext;
    }

    public async Task<Unit> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        var theme = await _profileContext.Themes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.Id);

        _profileContext.Remove(theme);
        await _profileContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
