using FluentValidation;
using System;

namespace Yuviron.Application.Features.StudioArtist.Profile.Commands.UpdateSocialLinks;

public sealed class UpdateSocialLinksValidator : AbstractValidator<UpdateSocialLinksCommand>
{
    public UpdateSocialLinksValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();

        RuleFor(x => x.Links)
            .Must(links => links == null || links.Count <= 10)
            .WithMessage("You can add a maximum of 10 social links.");

        RuleForEach(x => x.Links).ChildRules(link =>
        {
            link.RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Social link type is required.")
                .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");

            link.RuleFor(x => x.Url)
                .NotEmpty().WithMessage("URL is required.")
                .MaximumLength(500).WithMessage("URL cannot exceed 500 characters.")
                .Must(BeAValidUrl).WithMessage("Must be a valid HTTP or HTTPS URL.");
        });
    }

    private bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? outUri)
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}