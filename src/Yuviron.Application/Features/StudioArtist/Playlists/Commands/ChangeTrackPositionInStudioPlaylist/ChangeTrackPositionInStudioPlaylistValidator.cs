using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.ChangeTrackPositionInStudioPlaylist;

public sealed class ChangeTrackPositionInStudioPlaylistValidator : AbstractValidator<ChangeTrackPositionInStudioPlaylistCommand>
{
    public ChangeTrackPositionInStudioPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
    }
}