using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.DeletePlaylist;

public sealed class DeleteStudioPlaylistValidator : AbstractValidator<DeleteStudioPlaylistCommand>
{
    public DeleteStudioPlaylistValidator() => RuleFor(x => x.PlaylistId).NotEmpty();
}