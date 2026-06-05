using FluentValidation;

namespace Yuviron.Application.Features.Client.Marketing.Queries.ResolveSmartLink;

public sealed class ResolveSmartLinkValidator : AbstractValidator<ResolveSmartLinkQuery>
{
    public ResolveSmartLinkValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("SmartLink code is required.");
    }
}