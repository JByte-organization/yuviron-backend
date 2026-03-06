using FluentValidation;
using System;

namespace Yuviron.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Имя (Display name) обязательно.")
            .MaximumLength(100).WithMessage("Имя не может быть длиннее 100 символов.");

        RuleFor(x => x.Country)
            .Length(2).When(x => !string.IsNullOrWhiteSpace(x.Country))
            .WithMessage("Код страны должен состоять ровно из 2 символов (ISO).");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Описание (Bio) не может превышать 1000 символов.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Недопустимое значение пола.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Дата рождения обязательна.")
            .LessThan(DateTime.UtcNow.Date).WithMessage("Дата рождения не может быть в будущем.");
    }
}