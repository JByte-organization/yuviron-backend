using FluentValidation;

namespace Yuviron.Application.Features.Auth.Queries.CheckEmail;

public class CheckEmailValidator : AbstractValidator<CheckEmailQuery>
{
    public CheckEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email є обов'язковим.")
            .EmailAddress().WithMessage("Некоректний формат Email.");
    }
}