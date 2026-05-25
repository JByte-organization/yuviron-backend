using FluentValidation;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(300).WithMessage("Bio cannot exceed 300 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));
    }
}