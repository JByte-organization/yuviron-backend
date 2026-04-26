using FluentValidation;

namespace Yuviron.Application.Features.Analytics.Commands.StartTrackPlay;

public sealed class StartTrackPlayValidator : AbstractValidator<StartTrackPlayCommand>
{
    public StartTrackPlayValidator()
    {
        RuleFor(x => x.TrackId)
            .NotEmpty()
            .WithMessage("TrackId cannot be empty.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.");
    }
}