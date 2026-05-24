using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.UpdatePlaylist;

public sealed class UpdatePlaylistValidator : AbstractValidator<UpdatePlaylistCommand>
{
    public UpdatePlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Playlist title must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Invalid visibility status.")
            .When(x => x.Visibility.HasValue);
    }
}
