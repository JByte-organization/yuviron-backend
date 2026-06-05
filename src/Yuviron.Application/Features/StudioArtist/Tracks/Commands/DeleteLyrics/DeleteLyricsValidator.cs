using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteLyrics;

public sealed class DeleteLyricsValidator : AbstractValidator<DeleteLyricsCommand>
{
    public DeleteLyricsValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
    }
}