using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.RemovePlaylistFromFavorites;

public sealed class RemovePlaylistFromFavoritesValidator : AbstractValidator<RemovePlaylistFromFavoritesCommand>
{
    public RemovePlaylistFromFavoritesValidator()
    {
        RuleFor(x => x.PlaylistId).NotEqual(Guid.Empty);
    }
}