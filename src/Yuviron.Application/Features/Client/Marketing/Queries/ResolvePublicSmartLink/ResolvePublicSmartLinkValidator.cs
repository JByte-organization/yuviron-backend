using FluentValidation;

namespace Yuviron.Application.Features.Client.Marketing.Queries.ResolvePublicSmartLink;

public sealed class ResolvePublicSmartLinkValidator : AbstractValidator<ResolvePublicSmartLinkQuery>
{
    public ResolvePublicSmartLinkValidator()
    {
        RuleFor(x => x.EntityType).IsInEnum();
        RuleFor(x => x.PublicId).NotEmpty().MaximumLength(32);
    }
}
