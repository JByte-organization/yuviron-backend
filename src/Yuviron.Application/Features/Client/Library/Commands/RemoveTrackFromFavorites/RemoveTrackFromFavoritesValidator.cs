using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveTrackFromFavorites;

public sealed class RemoveTrackFromFavoritesValidator : AbstractValidator<RemoveTrackFromFavoritesCommand>
{
    public RemoveTrackFromFavoritesValidator()
    {
        RuleFor(x => x.TrackId).NotEqual(Guid.Empty);
    }
}
