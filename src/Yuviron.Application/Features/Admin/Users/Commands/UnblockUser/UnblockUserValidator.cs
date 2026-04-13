using FluentValidation;

namespace Yuviron.Application.Features.Admin.Users.Commands.UnblockUser;

public sealed class UnblockUserValidator : AbstractValidator<UnblockUserCommand>
{
    public UnblockUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty);
    }
}