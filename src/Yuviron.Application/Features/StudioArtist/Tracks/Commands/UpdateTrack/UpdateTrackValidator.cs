using FluentValidation;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Commands.UpdateTrack;

public sealed class UpdateTrackValidator : AbstractValidator<UpdateTrackCommand>
{
    public UpdateTrackValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty();
        
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleForEach(x => x.Collaborators).ChildRules(collaborator => 
        {
            collaborator.RuleFor(c => c.ArtistId).NotEmpty();
            collaborator.RuleFor(c => c.Role).IsInEnum();
        });
    }
}