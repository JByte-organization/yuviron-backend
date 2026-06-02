using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.DeleteAlbum;

public sealed class DeleteAlbumValidator : AbstractValidator<DeleteAlbumCommand>
{
    public DeleteAlbumValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEmpty()
            .WithMessage("Album ID is required.");
    }
}