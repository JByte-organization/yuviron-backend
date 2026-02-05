using FluentValidation;

namespace Yuviron.Application.Features.Auth.Commands.SendLoginCode;

public class SendLoginCodeValidator : AbstractValidator<SendLoginCodeCommand>
{
    public SendLoginCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}