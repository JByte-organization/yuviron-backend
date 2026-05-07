using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Playlist name is required.")
            .MaximumLength(100).WithMessage("Playlist name must not exceed 100 characters.");

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500).WithMessage("Cover URL is too long.")
            .When(x => !string.IsNullOrEmpty(x.CoverUrl));
    }
}