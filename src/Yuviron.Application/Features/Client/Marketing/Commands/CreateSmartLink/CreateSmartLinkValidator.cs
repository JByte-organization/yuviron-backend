using FluentValidation;

namespace Yuviron.Application.Features.Client.Marketing.Commands.CreateSmartLink;

public sealed class CreateSmartLinkValidator : AbstractValidator<CreateSmartLinkCommand>
{
    public CreateSmartLinkValidator()
    {
        RuleFor(x => x.EntityType).IsInEnum();
        RuleFor(x => x.EntityId).NotEmpty();
    }
}
