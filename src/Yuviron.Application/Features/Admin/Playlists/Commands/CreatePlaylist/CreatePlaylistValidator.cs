using FluentValidation;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(150).WithMessage("Title must not exceed 150 characters");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description is too long");
        
        RuleFor(v => v.CoverUrl)
            .MaximumLength(2048).WithMessage("Cover URL is too long")
            .Must(ValidationExtensions.BeValidUrl).When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("Cover URL must be a valid URI.");
    }
}