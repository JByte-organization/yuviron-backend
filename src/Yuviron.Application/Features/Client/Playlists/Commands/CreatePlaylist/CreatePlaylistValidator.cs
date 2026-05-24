using FluentValidation;

namespace Yuviron.Application.Features.Client.Playlists.Commands.CreatePlaylist;

public sealed class CreatePlaylistValidator : AbstractValidator<CreatePlaylistCommand>
{
    public CreatePlaylistValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Playlist title is required.")
            .MaximumLength(100).WithMessage("Playlist title must not exceed 100 characters.");
        
        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Invalid visibility status.");
    }
}
