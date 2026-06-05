using FluentValidation;

namespace Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;

public sealed class ApproveBannerRequestValidator : AbstractValidator<ApproveBannerRequestCommand>
{
    public ApproveBannerRequestValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}