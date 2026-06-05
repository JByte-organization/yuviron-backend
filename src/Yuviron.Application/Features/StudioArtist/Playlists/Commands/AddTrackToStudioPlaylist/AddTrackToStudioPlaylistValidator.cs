using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.AddTrackToStudioPlaylist;

public sealed class AddTrackToStudioPlaylistValidator : AbstractValidator<AddTrackToStudioPlaylistCommand>
{
    public AddTrackToStudioPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
    }
}