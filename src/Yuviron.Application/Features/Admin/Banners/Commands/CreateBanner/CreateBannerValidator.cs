using FluentValidation;

namespace Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;

public sealed class CreateBannerValidator : AbstractValidator<CreateBannerCommand>
{
    public CreateBannerValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BannerFileId).NotEmpty();
        RuleFor(x => x.TargetUrl).NotEmpty().MaximumLength(1000);
    }
}
