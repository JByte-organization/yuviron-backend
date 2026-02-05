using FluentValidation;

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public class LoginWithCodeValidator : AbstractValidator<LoginWithCodeCommand>
{
    public LoginWithCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Введіть код підтвердження.")
            .Length(6).WithMessage("Код має містити рівно 6 цифр.")
            .Matches("^[0-9]*$").WithMessage("Код має складатися лише з цифр.");
    }
}