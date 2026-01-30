using FluentValidation;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        // 1. Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат Email.")
            .MaximumLength(320).WithMessage("Email слишком длинный.");

        // 2. Пароль
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(6).WithMessage("Пароль должен быть длиннее 6 символов.")
            .MaximumLength(100);

        // 3. Имя (для профиля)
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .MaximumLength(50).WithMessage("Имя не может быть длиннее 50 символов.");
    }
}