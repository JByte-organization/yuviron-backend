using FluentValidation;

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => BeAtLeast16YearsOld(dob, timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage("User must be at least 16 years old.");

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.AccountState)
            .IsInEnum();

        RuleFor(x => x.RoleIds)
            .Must(HaveDistinctRoles)
            .When(x => x.RoleIds is not null)
            .WithMessage("RoleIds must not contain duplicates.");

        RuleForEach(x => x.RoleIds!)
            .NotEqual(Guid.Empty)
            .When(x => x.RoleIds is not null);
    }

    private static bool BeAtLeast16YearsOld(DateTime dateOfBirth, DateTime utcNow)
    {
        return dateOfBirth.Date <= utcNow.Date.AddYears(-16);
    }

    private static bool HaveDistinctRoles(IReadOnlyCollection<Guid>? roleIds)
    {
        return roleIds is null || roleIds.Distinct().Count() == roleIds.Count;
    }
}