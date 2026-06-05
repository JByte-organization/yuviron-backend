using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.UpdatePlaylist;

public sealed class UpdateStudioPlaylistValidator : AbstractValidator<UpdateStudioPlaylistCommand>
{
    public UpdateStudioPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Title));
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.Visibility).IsInEnum().When(x => x.Visibility.HasValue);
    }
}