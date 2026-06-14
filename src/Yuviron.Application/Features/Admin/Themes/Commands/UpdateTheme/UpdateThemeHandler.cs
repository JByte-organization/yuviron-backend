using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Commands.UpdateTheme;

public sealed class UpdateThemeHandler : IRequestHandler<UpdateThemeCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly IProfileContext _profileContext;

    public UpdateThemeHandler(IIdentityContext identityContext, IProfileContext profileContext)
    {
        _identityContext = identityContext;
        _profileContext = profileContext;
    }

    public async Task<Unit> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        var theme = await _profileContext.Themes
            .FirstOrDefaultAsync(x => x.Id == request.ThemeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Theme), request.ThemeId);

        if (request.UserId.HasValue)
        {
            var userExists = await _identityContext.Users.AnyAsync(x => x.Id == request.UserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException(nameof(User), request.UserId.Value);
            }
        }

        var normalizedName = request.Name.Trim();
        var duplicateExists = await _profileContext.Themes
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

        await _identityContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
