using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.UpdateAlbum;

public sealed class UpdateAlbumValidator : AbstractValidator<UpdateAlbumCommand>
{
    public UpdateAlbumValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);
        
        RuleFor(x => x.Description)
            .MaximumLength(2000) 
            .When(x => x.Description is not null);

        RuleFor(x => x.ReleaseType)
            .IsInEnum();
        
        RuleFor(x => x.ReleaseDate)
            .NotEqual(default(DateTime));
    }
}