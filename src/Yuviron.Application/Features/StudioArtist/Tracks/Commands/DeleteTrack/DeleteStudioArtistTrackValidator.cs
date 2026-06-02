using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed class DeleteStudioArtistTrackValidator : AbstractValidator<DeleteStudioArtistTrackCommand>
{
    public DeleteStudioArtistTrackValidator()
    {
        RuleFor(x => x.TrackId)
            .NotEqual(Guid.Empty);
    }
}
