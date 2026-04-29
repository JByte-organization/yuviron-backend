using FluentValidation;

namespace Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;

public sealed class CreateBannerValidator : AbstractValidator<CreateBannerCommand>
{
    public CreateBannerValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.BannerUrl)
            .NotEmpty().WithMessage("Banner image URL is required.")
            .MaximumLength(1000)
            .Must(url => !url.Contains("..")).WithMessage("Invalid file path.");

        RuleFor(x => x.TargetUrl)
            .NotEmpty().WithMessage("Target URL is required.")
            .MaximumLength(1000);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater.");
        //requerierd
    }
}