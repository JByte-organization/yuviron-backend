using FluentValidation;

namespace Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;

public sealed class UpdateBannerValidator : AbstractValidator<UpdateBannerCommand>
{
    public UpdateBannerValidator()
    {
        RuleFor(x => x.BannerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetUrl).NotEmpty().MaximumLength(1000);
    }
}
