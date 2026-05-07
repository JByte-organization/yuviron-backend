using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.DeletePlaylist;

public sealed class DeletePlaylistValidator : AbstractValidator<DeletePlaylistCommand>
{
    public DeletePlaylistValidator()
    {
        RuleFor(x => x.PlaylistId).NotEmpty();
    }
}