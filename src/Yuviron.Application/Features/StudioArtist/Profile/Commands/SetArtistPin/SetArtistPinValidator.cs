using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.SetArtistPin;

public sealed class SetArtistPinValidator : AbstractValidator<SetArtistPinCommand>
{
    public SetArtistPinValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.EntityType).IsInEnum().WithMessage("Invalid entity type for pin.");
        RuleFor(x => x.EntityId).NotEmpty().WithMessage("Entity ID to pin is required.");
        RuleFor(x => x.Position).GreaterThan(0).WithMessage("Position must be greater than 0.");
    }
}