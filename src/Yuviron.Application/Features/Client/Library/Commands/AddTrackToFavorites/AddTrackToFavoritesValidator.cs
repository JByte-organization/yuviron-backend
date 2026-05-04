using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.AddTrackToFavorites;

public sealed class AddTrackToFavoritesValidator : AbstractValidator<AddTrackToFavoritesCommand>
{
    public AddTrackToFavoritesValidator()
    {
        RuleFor(x => x.TrackId).NotEqual(Guid.Empty);
    }
}