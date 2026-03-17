using FluentValidation;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistValidator : AbstractValidator<UpdatePlaylistCommand>
{
    public UpdatePlaylistValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(150);

        RuleFor(v => v.Description)
            .MaximumLength(2000);
        
        RuleFor(v => v.CoverUrl)
            .MaximumLength(2048).WithMessage("Cover URL is too long")
            .Must(ValidationExtensions.BeValidUrl).When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("Cover URL must be a valid URI.");
    }
}