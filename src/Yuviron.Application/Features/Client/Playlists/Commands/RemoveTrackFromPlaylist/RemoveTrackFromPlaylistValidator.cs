using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed class RemoveTrackFromPlaylistValidator : AbstractValidator<RemoveTrackFromPlaylistCommand>
{
    public RemoveTrackFromPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
    }
}