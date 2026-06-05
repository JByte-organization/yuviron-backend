using FluentValidation;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrackLyrics;

public sealed class UpdateTrackLyricsValidator : AbstractValidator<UpdateTrackLyricsCommand>
{
    public UpdateTrackLyricsValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.LyricsText).MaximumLength(15000); 
    }
}