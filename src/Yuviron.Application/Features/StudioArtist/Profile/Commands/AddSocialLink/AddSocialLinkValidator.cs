using FluentValidation;
using System;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.AddSocialLink;

public sealed class AddSocialLinkValidator : AbstractValidator<AddSocialLinkCommand>
{
    public AddSocialLinkValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid social link type.");
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(500).WithMessage("URL cannot exceed 500 characters.")
            .Must(BeAValidUrl).WithMessage("Must be a valid HTTP or HTTPS URL.");
    }

    private bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? outUri)
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}