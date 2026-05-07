using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.AddTrackToPlaylist;

public sealed class AddTrackToPlaylistValidator : AbstractValidator<AddTrackToPlaylistCommand>
{
    public AddTrackToPlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
        RuleFor(x => x.TrackId).NotEmpty();
    }
}