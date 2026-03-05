using FluentValidation;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.DeleteTrack;

public sealed class DeleteTrackCommandValidator : AbstractValidator<DeleteTrackCommand>
{
    public DeleteTrackCommandValidator()
    {
        RuleFor(x => x.TrackId).NotEqual(Guid.Empty);
    }
}