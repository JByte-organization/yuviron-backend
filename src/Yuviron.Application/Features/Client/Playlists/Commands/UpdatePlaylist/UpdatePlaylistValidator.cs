using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistValidator : AbstractValidator<UpdatePlaylistCommand>
{
    public UpdatePlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Playlist name must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500).WithMessage("Cover URL is too long.")
            .When(x => !string.IsNullOrEmpty(x.CoverUrl));
    }
}