using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackValidator : AbstractValidator<DeleteTrackCommand>
{
    public DeleteTrackValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
    }
}