using System;
using FluentValidation;

namespace Yuviron.Application.Features.Admin.Ads.Commands.UpdateAd;

public sealed class UpdateAdValidator : AbstractValidator<UpdateAdCommand>
{
    public UpdateAdValidator()
    {
        RuleFor(x => x.AdId).NotEmpty().WithMessage("Ad Id is required.");
        
        RuleFor(x => x.AdvertiserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClickUrl).MaximumLength(500)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ClickUrl))
            .WithMessage("Click URL must be a valid absolute URL.");
    }
}