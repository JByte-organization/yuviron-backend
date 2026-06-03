using FluentValidation;

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.ClaimProfile;

public sealed class ClaimArtistProfileValidator : AbstractValidator<ClaimArtistProfileCommand>
{
    public ClaimArtistProfileValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEmpty().WithMessage("Artist ID is required.");

        RuleFor(x => x.ClaimedRole)
            .IsInEnum().WithMessage("Invalid role.");

        RuleFor(x => x.OfficialEmail)
            .NotEmpty().WithMessage("Official contact email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Links)
            .NotEmpty().WithMessage("Please provide links to official social media or websites.")
            .MaximumLength(1000);

        RuleFor(x => x.Message)
            .MaximumLength(500);
    }
}