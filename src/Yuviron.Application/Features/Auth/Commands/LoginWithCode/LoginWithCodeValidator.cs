using FluentValidation;

namespace Yuviron.Application.Features.Auth.Commands.LoginWithCode;

public sealed class LoginWithCodeValidator : AbstractValidator<LoginWithCodeCommand>
{
    public LoginWithCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(320).WithMessage("Email cannot exceed 320 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Code must be exactly 6 characters long.")
            .Matches("^[0-9]*$").WithMessage("Code must contain only digits.");
    }
}