using FluentValidation;

namespace Yuviron.Application.Features.Auth.Queries.CheckEmail;

public sealed class CheckEmailValidator : AbstractValidator<CheckEmailQuery>
{
    public CheckEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(320).WithMessage("Email cannot exceed 320 characters.");
    }
}