using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistValidator : AbstractValidator<AddTrackToPlaylistCommand>
{
    public AddTrackToPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty().WithMessage("Playlist ID is required.");
        RuleFor(x => x.TrackId).NotEmpty().WithMessage("Track ID is required.");
    }
}