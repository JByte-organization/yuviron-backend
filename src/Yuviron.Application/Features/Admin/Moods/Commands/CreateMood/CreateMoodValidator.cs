using FluentValidation;

namespace Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;

public sealed class CreateMoodValidator : AbstractValidator<CreateMoodCommand>
{
    public CreateMoodValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(v => v.CoverUrl)
            .MaximumLength(2000).WithMessage("Cover URL is too long");
    }
}