using FluentValidation;

namespace Yuviron.Application.Features.Admin.Artists.Commands.CreateArtist;

public sealed class CreateArtistCommandValidator : AbstractValidator<CreateArtistCommand>
{
    public CreateArtistCommandValidator()
    {
        RuleFor(x => x.OwnerUserId)
            .NotEqual(Guid.Empty)
            .When(x => x.OwnerUserId.HasValue);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Bio)
            .MaximumLength(2000)
            .When(x => x.Bio is not null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(2048)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        RuleFor(x => x.BannerUrl)
            .MaximumLength(2048)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.BannerUrl));

        RuleFor(x => x.VerificationStatus)
            .IsInEnum();
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}