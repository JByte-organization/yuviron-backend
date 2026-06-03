using FluentValidation;

namespace Yuviron.Application.Features.ArtistDashboard.Profiles.Commands.CreateProfile;

public sealed class CreateArtistProfileValidator : AbstractValidator<CreateArtistProfileCommand>
{
    public CreateArtistProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Artist name is required.")
            .MaximumLength(200).WithMessage("Artist name cannot exceed 200 characters.");

    }
}