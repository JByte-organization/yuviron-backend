using FluentValidation;

namespace Yuviron.Application.Features.Auth.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")
            .EmailAddress().WithMessage("Некорректный формат Email.")
            .MaximumLength(320).WithMessage("Email слишком длинный.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(6).WithMessage("Пароль должен быть длиннее 6 символов.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .MaximumLength(50).WithMessage("Имя не может быть длиннее 50 символов.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-16))
            .WithMessage("Вам должно быть не менее 16 лет для регистрации.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Выберите пол из списка.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("Вы должны согласиться с Политикой конфиденциальности.");
    }
}