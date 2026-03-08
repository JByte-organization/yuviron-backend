using System;
using System.Linq;
using FluentValidation;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed class UpdateGenreValidator : AbstractValidator<UpdateGenreCommand>
{
    public UpdateGenreValidator()
    {
        RuleFor(x => x.GenreId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .Must(name => name != null && name.Any(char.IsLetter))
            .WithMessage("Genre name must contain at least one letter.");

        RuleFor(x => x.CoverUrl)
            .MaximumLength(2048)
            .Must(ValidationExtensions.BeValidUrl).When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("Cover URL must be a valid URI.");

    }

}