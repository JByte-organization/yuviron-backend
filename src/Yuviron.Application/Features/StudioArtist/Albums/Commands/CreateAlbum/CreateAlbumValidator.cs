using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.CreateAlbum;

public sealed class CreateAlbumValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty()
            .WithMessage("Artist ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);
        
        RuleFor(x => x.Description)
            .MaximumLength(2000) 
            .When(x => x.Description is not null);

        RuleFor(x => x.ReleaseType)
            .IsInEnum();
    }
}