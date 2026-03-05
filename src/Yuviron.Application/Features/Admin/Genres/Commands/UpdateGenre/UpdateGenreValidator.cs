using System;
using System.Linq;
using FluentValidation;

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
            .MaximumLength(500)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("Cover URL must be a valid URI.");

        RuleFor(x => x.HexColor)
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").When(x => !string.IsNullOrEmpty(x.HexColor))
            .WithMessage("Hex color must be a valid hex code (e.g., #FFFFFF or #FFF).");
    }

    private bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var outUri) 
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}