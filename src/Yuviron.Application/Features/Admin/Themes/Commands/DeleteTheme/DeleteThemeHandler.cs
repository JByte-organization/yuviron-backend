using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Commands.DeleteTheme;

public sealed class DeleteThemeHandler : IRequestHandler<DeleteThemeCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteThemeHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        var theme = await _context.Themes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.Id);

        _context.Themes.Remove(theme);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
