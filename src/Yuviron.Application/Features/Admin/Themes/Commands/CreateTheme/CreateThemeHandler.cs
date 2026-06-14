using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Commands.CreateTheme;

public sealed class CreateThemeHandler : IRequestHandler<CreateThemeCommand, Guid>
{
    private readonly IIdentityContext _identityContext;
    private readonly IProfileContext _profileContext;

    public CreateThemeHandler(IIdentityContext identityContext, IProfileContext profileContext)
    {
        _identityContext = identityContext;
        _profileContext = profileContext;
    }

    public async Task<Guid> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        if (request.UserId.HasValue)
        {
            var userExists = await _identityContext.Users.AnyAsync(x => x.Id == request.UserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException(nameof(User), request.UserId.Value);
            }
        }

        var duplicateExists = await _profileContext.Themes
            .AnyAsync(x => x.UserId == request.UserId && x.Name == normalizedName, cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"Theme '{normalizedName}' already exists for this owner.");
        }

        var theme = Theme.Create(
            normalizedName,
            request.PrimaryColor.Trim(),
            request.SecondaryColor.Trim(),
            request.BackgroundColor.Trim(),
            isSystem: !request.UserId.HasValue,
            isPremiumOnly: request.IsPremiumOnly,
            userId: request.UserId);

        _profileContext.Add(theme);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return theme.Id;
    }
}
