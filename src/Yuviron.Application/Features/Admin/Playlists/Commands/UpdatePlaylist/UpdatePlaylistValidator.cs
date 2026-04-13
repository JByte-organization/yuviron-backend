using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistValidator : AbstractValidator<UpdatePlaylistCommand>
{
    public UpdatePlaylistValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        
        RuleFor(x => x.OwnerUserId)
            .NotEmpty()
            .When(x => !x.IsEditorial)
            .WithMessage("OwnerUserId is required for non-editorial playlists.");

        RuleFor(x => x.OwnerUserId)
            .Must(x => x == null || x == Guid.Empty)
            .When(x => x.IsEditorial)
            .WithMessage("Editorial playlists cannot have an owner.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(150);

        RuleFor(v => v.Description)
            .MaximumLength(2000);
        
        RuleFor(v => v.CoverUrl)
            .MaximumLength(2048).WithMessage("Cover URL is too long")
            .Must(url => url == null || !url.Contains(".."))
            .WithMessage("Invalid file path.")
            .When(x => !string.IsNullOrEmpty(x.CoverUrl));
    }
}