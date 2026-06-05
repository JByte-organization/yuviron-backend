using FluentValidation;

namespace Yuviron.Application.Features.Admin.Banners.Commands.RejectBannerRequest;

public sealed class RejectBannerRequestValidator : AbstractValidator<RejectBannerRequestCommand>
{
    public RejectBannerRequestValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}