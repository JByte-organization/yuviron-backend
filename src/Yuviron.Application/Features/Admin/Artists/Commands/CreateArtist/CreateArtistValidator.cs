using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed class CreateArtistCommandValidator : AbstractValidator<CreateArtistCommand>
{
    public CreateArtistCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEqual(Guid.Empty).When(x => x.OwnerUserId.HasValue);

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