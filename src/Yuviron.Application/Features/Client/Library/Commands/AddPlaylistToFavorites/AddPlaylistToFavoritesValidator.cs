using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.AddPlaylistToFavorites;

public sealed class AddPlaylistToFavoritesValidator : AbstractValidator<AddPlaylistToFavoritesCommand>
{
    public AddPlaylistToFavoritesValidator()
    {
        RuleFor(x => x.PlaylistId).NotEqual(Guid.Empty);
    }
}