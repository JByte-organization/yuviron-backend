using System;
using System.Linq;
using FluentValidation;

namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;

public sealed class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Genre name is required.")
            .MinimumLength(2).WithMessage("Genre name must be at least 2 characters long.")
            .MaximumLength(50).WithMessage("Genre name must not exceed 50 characters.")
            .Must(name => name != null && name.Any(char.IsLetter))
            .WithMessage("Genre name must contain at least one letter.");

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500).WithMessage("Cover URL is too long.")
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