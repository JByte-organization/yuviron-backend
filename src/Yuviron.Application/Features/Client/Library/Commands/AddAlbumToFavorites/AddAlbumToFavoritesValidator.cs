using FluentValidation;

namespace Yuviron.Application.Features.Client.Library.Commands.AddAlbumToFavorites;

public sealed class AddAlbumToFavoritesValidator : AbstractValidator<AddAlbumToFavoritesCommand>
{
    public AddAlbumToFavoritesValidator()
    {
        RuleFor(x => x.AlbumId).NotEqual(Guid.Empty);
    }
}