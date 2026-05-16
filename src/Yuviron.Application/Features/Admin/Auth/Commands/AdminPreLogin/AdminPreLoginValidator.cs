using FluentValidation;

namespace Yuviron.Application.Features.Admin.Auth.Commands.AdminPreLogin;

public sealed class AdminPreLoginValidator : AbstractValidator<AdminPreLoginCommand>
{
    public AdminPreLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}