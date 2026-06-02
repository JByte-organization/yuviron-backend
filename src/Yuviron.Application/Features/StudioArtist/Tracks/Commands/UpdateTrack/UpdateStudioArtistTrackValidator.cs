using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

public sealed class UpdateStudioArtistTrackValidator : AbstractValidator<UpdateStudioArtistTrackCommand>
{
    public UpdateStudioArtistTrackValidator()
    {
        RuleFor(x => x.TrackId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleForEach(x => x.CoAuthorIds!)
            .NotEqual(Guid.Empty)
            .When(x => x.CoAuthorIds is { Count: > 0 });
    }
}
