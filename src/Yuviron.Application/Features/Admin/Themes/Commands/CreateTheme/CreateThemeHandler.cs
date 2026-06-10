using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Themes.Commands.CreateTheme;

public sealed class CreateThemeHandler : IRequestHandler<CreateThemeCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateThemeHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        if (request.UserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == request.UserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException(nameof(User), request.UserId.Value);
            }
        }

        var duplicateExists = await _context.Themes
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

        _context.Themes.Add(theme);
        await _context.SaveChangesAsync(cancellationToken);

        return theme.Id;
    }
}
