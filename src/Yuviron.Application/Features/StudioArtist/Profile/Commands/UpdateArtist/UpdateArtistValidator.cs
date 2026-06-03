using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistValidator : AbstractValidator<UpdateArtistCommand>
{
    public UpdateArtistValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio != null);
    }
}