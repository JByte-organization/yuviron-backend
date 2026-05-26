using FluentValidation;

namespace Yuviron.Application.Features.Admin.Ads.Commands.CreateAd;

public sealed class CreateAdValidator : AbstractValidator<CreateAdCommand>
{
    public CreateAdValidator()
    {
        RuleFor(x => x.AdvertiserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AudioFileId).NotEmpty();
        RuleFor(x => x.ImageFileId).NotEmpty();
        RuleFor(x => x.ClickUrl).MaximumLength(500)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ClickUrl))
            .WithMessage("Click URL must be a valid absolute URL.");
    }
}