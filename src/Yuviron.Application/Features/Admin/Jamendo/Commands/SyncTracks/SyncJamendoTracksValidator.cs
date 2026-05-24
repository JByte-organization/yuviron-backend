using FluentValidation;

namespace Yuviron.Application.Features.Admin.Jamendo.Commands.SyncTracks;

public sealed class SyncJamendoTracksValidator : AbstractValidator<SyncJamendoTracksCommand>
{
    public SyncJamendoTracksValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithMessage("The number of tracks to sync must be greater than zero.")
            .LessThanOrEqualTo(50)
            .WithMessage("You cannot request more than 50 tracks at a time to avoid overloading the server and the Jamendo API.");
    }
}