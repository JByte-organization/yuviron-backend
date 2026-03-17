using FluentValidation;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed class RemoveTrackFromPlaylistValidator : AbstractValidator<RemoveTrackFromPlaylistCommand>
{
    public RemoveTrackFromPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId)
            .NotEmpty().WithMessage("Playlist ID is required.");

        RuleFor(x => x.TrackId)
            .NotEmpty().WithMessage("Track ID is required.");
    }
}