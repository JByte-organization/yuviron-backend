using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.RemoveArtistPin;

public sealed class RemoveArtistPinValidator : AbstractValidator<RemoveArtistPinCommand>
{
    public RemoveArtistPinValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Position).GreaterThan(0).WithMessage("Position must be greater than 0.");
    }
}