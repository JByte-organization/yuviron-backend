using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.UpdateArtist;

public sealed class UpdateArtistCommandValidator : AbstractValidator<UpdateArtistCommand>
{
    public UpdateArtistCommandValidator()
    {
        RuleFor(x => x.ArtistId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Bio)
            .MaximumLength(2000)
            .When(x => x.Bio is not null);

        RuleFor(x => x.VerificationStatus)
            .IsInEnum();
    }
}