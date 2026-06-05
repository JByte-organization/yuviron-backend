using FluentValidation;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrackLyrics;

public sealed class DeleteTrackLyricsValidator : AbstractValidator<DeleteTrackLyricsCommand>
{
    public DeleteTrackLyricsValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
    }
}