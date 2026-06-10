using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Commands.UpdateTheme;

public sealed class UpdateThemeHandler : IRequestHandler<UpdateThemeCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateThemeHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        var theme = await _context.Themes
            .FirstOrDefaultAsync(x => x.Id == request.ThemeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.ThemeId);

        if (request.UserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == request.UserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException(nameof(User), request.UserId.Value);
            }
        }

        var normalizedName = request.Name.Trim();
        var duplicateExists = await _context.Themes
            .AnyAsync(x =>
                x.Id != request.ThemeId &&
                x.UserId == request.UserId &&
                x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"Theme '{normalizedName}' already exists for this owner.");
        }

        theme.Update(
            normalizedName,
            request.PrimaryColor.Trim(),
            request.SecondaryColor.Trim(),
            request.BackgroundColor.Trim(),
            isSystem: !request.UserId.HasValue,
            isPremiumOnly: request.IsPremiumOnly,
            userId: request.UserId);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
