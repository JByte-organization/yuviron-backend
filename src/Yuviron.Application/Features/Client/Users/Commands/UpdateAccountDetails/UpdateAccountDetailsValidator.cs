using FluentValidation;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateAccountDetails;

public sealed class UpdateAccountDetailsValidator : AbstractValidator<UpdateAccountDetailsCommand>
{
    public UpdateAccountDetailsValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Country)
            .MaximumLength(2).WithMessage("Country must be a 2-letter code.")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.DateOfBirth)
            .Must(BeAtLeast16YearsOld).WithMessage("You must be at least 16 years old.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Please select a valid gender.");
    }

    private static bool BeAtLeast16YearsOld(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        return dateOfBirth.Date <= today.AddYears(-16);
    }
}
