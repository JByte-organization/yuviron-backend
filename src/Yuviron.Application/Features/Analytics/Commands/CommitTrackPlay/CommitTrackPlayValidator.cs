using FluentValidation;

namespace Yuviron.Application.Features.Analytics.Commands.CommitTrackPlay;

public sealed class CommitTrackPlayValidator : AbstractValidator<CommitTrackPlayCommand>
{
    public CommitTrackPlayValidator()
    {
        RuleFor(x => x.PlaySessionId)
            .NotEmpty()
            .WithMessage("PlaySessionId cannot be empty.");

        RuleFor(x => x.TrackId)
            .NotEmpty()
            .WithMessage("TrackId cannot be empty.");

        RuleFor(x => x.ArtistId)
            .NotEmpty()
            .WithMessage("ArtistId cannot be empty.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.");
    }
}