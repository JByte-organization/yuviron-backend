using FluentValidation;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;

public sealed class CreateMoodValidator : AbstractValidator<CreateMoodCommand>
{
    public CreateMoodValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(v => v.CoverUrl)
            .MaximumLength(2048).WithMessage("Cover URL is too long")
            .Must(ValidationExtensions.BeValidUrl).When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("Cover URL must be a valid URI.");
    }
}