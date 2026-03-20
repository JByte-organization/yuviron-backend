using FluentValidation;

namespace Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;

public sealed class UpdateMoodCommandValidator : AbstractValidator<UpdateMoodCommand>
{
    public UpdateMoodCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("Id is required");
        
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(v => v.CoverUrl)
            .MaximumLength(2048).WithMessage("Cover URL is too long")
            .Must(url => url == null || !url.Contains(".."))
            .WithMessage("Invalid file path.")
            .When(x => !string.IsNullOrEmpty(x.CoverUrl));
    }
}