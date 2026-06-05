using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.RemoveTrackFromStudioPlaylist;

public sealed class RemoveTrackFromStudioPlaylistValidator : AbstractValidator<RemoveTrackFromStudioPlaylistCommand>
{
    public RemoveTrackFromStudioPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
    }
}