using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.CreatePlaylist;

public sealed class CreateStudioPlaylistValidator : AbstractValidator<CreateStudioPlaylistCommand>
{
    public CreateStudioPlaylistValidator()
    {
        RuleFor(x => x.ArtistId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Visibility).IsInEnum();
    }
}