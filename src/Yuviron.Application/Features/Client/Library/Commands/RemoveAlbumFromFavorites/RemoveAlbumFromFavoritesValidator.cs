using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.RemoveAlbumFromFavorites;

public sealed class RemoveAlbumFromFavoritesValidator : AbstractValidator<RemoveAlbumFromFavoritesCommand>
{
    public RemoveAlbumFromFavoritesValidator()
    {
        RuleFor(x => x.AlbumId).NotEqual(Guid.Empty);
    }
}