using FluentValidation;

namespace Yuviron.Application.Features.Admin.Auth.Commands.AdminLogin;

public sealed class AdminLoginValidator : AbstractValidator<AdminLoginCommand>
{
    public AdminLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Code must be exactly 6 characters long.")
            .Matches("^[0-9]*$").WithMessage("Code must contain only digits.");
    }
}