using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistValidator()
    {
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
            .MaximumLength(150).WithMessage("Title must not exceed 150 characters");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Description is too long");
    }
}