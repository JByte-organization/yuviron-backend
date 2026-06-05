using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateLyrics;

public sealed class UpdateLyricsValidator : AbstractValidator<UpdateLyricsCommand>
{
    public UpdateLyricsValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
        RuleFor(x => x.LyricsText).MaximumLength(15000); 
    }
}