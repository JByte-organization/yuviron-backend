using FluentValidation;

namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;

public sealed class CreateGenreValidator : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Genre name is required.")
            .MinimumLength(2).WithMessage("Genre name must be at least 2 characters long.")
            .MaximumLength(50).WithMessage("Genre name must not exceed 50 characters.")
            .Must(name => name != null && name.Any(char.IsLetter))
            .WithMessage("Genre name must contain at least one letter.");
    }
}